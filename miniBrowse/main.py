#I'm making a few more global variables: location, document, options, and stateStack
#are all now global. It occurs to me that I could wrap this whole thing inside a class.
#all the globals act like attributes of an instance, so wrapping it in a class could
#offer tab functionality (multiple instances). I'm not really focused on that right now
#though. I'm decided to make these changes to implement back functionality.

#reformat primary functions to update state instead of returning values!! Limit use of globals
#inside primary functions. Let update state do the updating

#consider optimizations for when you're only changing a single global state variable

# Next step is to continue work on the doRange function
# * * * * Add threading to get images on a page much faster
# * * * * Add additional optional arguments to be passed for functions in range
# * * * * Add secondary actions to be performed within actions (get images within new pages)

#after that, implement flag system and optional arguments that may
#be passed with main commands.

import requests
from bs4 import BeautifulSoup
import sys
from ActionDict import ActionDict, Stack
import os
import shutil
""" Global declarations """
global location
global document
global options
global stateStack
global forwardStateStack
""" Primary Functions """


def cd(link=None):
  """
    WARNING!! THIS IS STILL UNDER DEVELOPMENT AND IS NOT STABLE
    """

  global document
  global location

  localOptions = ActionDict()

  if not link:
    link = input("Enter a URL: ")
  #If there are bugs here with multiple exceptions, consider adding break statements
  try:
    document = BeautifulSoup(requests.get(link).text, "html.parser")
    location = link
  except requests.exceptions.InvalidURL:
    localOptions.update({"1": (cd)})
    localOptions.actionString = f"Invalid URL {link}\n\
                                1: manually enter new url"

  except requests.exceptions.MissingSchema:
    localOptions.update({
        "1": (cd, "https://" + link),
        "2": (cd, "https://" + link)
    })
    localOptions.actionString = "The URL requires a schema\n\
                                1: run over https\n\
                                2: run over http"

  except requests.exceptions.ConnectionError:
    print("Connection error")
  except requests.exceptions.ConnectTimeout:
    print("Connection timeout")
  print(localOptions.actionString)
  updateState(location, document, localOptions)


def ls(**kwargs):
  """
    TEXT IN {} INDICATES POTENTIALLY UNRELIABLE INFORMATION

    print a list of navigable places and accessible files and return a dictionary with the 
    required string, function pointer, and arguments for said pointer.
    
    """

  global document
  global location
  localOptions = ActionDict()
  optionsID = 1

  # strings are appended to optionsString, joined at the end
  # and then sent along with the return actions dict
  optionsString = list()

  optionsString.append(f"{document.title.string} at {location}\n")
  #get navigable text links
  links = document.css.select("a")
  #reformat internal links
  for link in links:
    href = link.get("href")
    link["href"] = reformatLink(href)

  textLinks = [link for link in links
               if link.get("href") and link.string]  #filter for text links
  if len(textLinks) > 0:
    optionsString.append(
        "___________________________________________________________\n")
  for link in textLinks:
    localOptions[str(optionsID)] = (cd, link.get("href"))
    optionsString.append(f"{optionsID}: {link.string}\n")
    optionsID += 1

  #get navigable non-text links
  nonTextLinks = [
      link for link in links if link.get("href") and not link.string
  ]  #filter for text links
  if len(nonTextLinks) > 0:
    optionsString.append("\nLink text not available\n")
    optionsString.append(
        "___________________________________________________________\n")
  for link in nonTextLinks:
    localOptions[str(optionsID)] = (cd, link.get("href"))
    if len(str(link)) > 100:
      optionsString.append(f"{optionsID}: {str(link)[:100]}...\n")
      optionsID += 1
      localOptions[optionsID] = (print, str(link))
      optionsString.append(f"*******{optionsID}: print entire link tag\n")
    else:
      optionsString.append(f"{optionsID}: {str(link)}\n")
    optionsID += 1

  #get image with alt text
  imgs = document.css.select("img")
  #reformat links (fix internals)
  for img in imgs:
    src = img.get("src")
    img["src"] = reformatLink(src)

  printableImages = list(filter(lambda x: x.get("alt") and x.get("src"), imgs))
  if len(printableImages) > 0:
    optionsString.append("\nNice images\n")
    optionsString.append(
        "___________________________________________________________\n")
  for img in printableImages:
    localOptions[str(optionsID)] = (get, img.get("src"))
    optionsString.append(f"{optionsID}: {img.get('alt')}")
    #Try to print the file type
    ext = fileExtensionOf(img.get("src"))
    if len(ext) > 0:
      ext = ext[1:]  # don't include the .
    optionsString.append(ext)
    optionsString.append("\n")
    optionsID += 1

  #get images without alt text
  otherImgs = list(filter(lambda x: not x.get("alt") and x.get("src"), imgs))
  if len(otherImgs) > 0:
    optionsString.append("\nLess nice images\n")
    optionsString.append(
        "___________________________________________________________\n")
  for img in otherImgs:
    localOptions[str(optionsID)] = (get, img.get("src"))
    if len(str(img)) > 100:
      optionsString.append(f"{optionsID}: {str(img)[:100]}...\n")
      optionsID += 1
      localOptions[optionsID] = (print, str(img))
      optionsString.append(
          f"*******{str(optionsID)}: print entire image tag\n")

    else:
      optionsString.append(f"{optionsID}: {str(img)[:100]}...\n")
      #Try to print the file type
      ext = fileExtensionOf(img.get("src"))
      if len(ext) > 0:
        ext = ext[1:]  # don't include the .
      optionsString.append(ext)
      optionsString.append("\n")
    optionsID += 1
  localOptions.actionString = "".join(optionsString)
  print(localOptions.actionString)
  updateState(location, document, localOptions)


def get(url, downloadDir="images", lengthLimit=35):
  """
    Downloads resources to local machine

    {Not implemented yet}
    """
  global location
  global document
  global options
  print(f"attempting to download from {url}")
  dir = os.listdir()
  if downloadDir not in dir:
    try:
      os.mkdir(downloadDir)
    except Exception as e:
      print(str(e))
      return
  os.chdir(downloadDir)
  ext = fileExtensionOf(url)
  if len(ext) < 1:
    print("Unable to download, no file extension")
    return

  # contains file names (w/o extensions) of files in downloadDir of same type
  filesInDir = [
      x[:-len(ext)]
      for x in filter(lambda y: y[-len(ext):] == ext, os.listdir())
  ]

  #generate filename
  startIndex = url.rfind("/")
  extIndex = url.rfind(ext)
  if startIndex != -1 and len(url) > startIndex + 1:
    # I think this shouldn't include any characters from the file extension
    fileName = (url[startIndex + 1:extIndex]
                if len(url[startIndex + 1:extIndex]) <= lengthLimit else
                url[startIndex + 1:startIndex + 1 + lengthLimit])
  elif len(url[:extIndex]) > 0:
    fileName = url[:extIndex]
  else:
    fileName = "unknown"

  disallowedCharacters = set({
      "#", "%", "&", "{", "}", "/", "<", ">", "*", "?", "\\", " ", "$", "!",
      "'", "\"", ":", "@", "+", "`", "|", "="
  })
  fixedFileNameList = list()
  for char in fileName:
    if char not in disallowedCharacters:
      fixedFileNameList.append(char)
    else:
      fixedFileNameList.append("-")
  fileName = "".join(fixedFileNameList)

  appendInt = 1
  if fileName in filesInDir:
    fileName = fileName + f"({str(appendInt)})"

  while fileName in filesInDir:  # this loop might cause slowdowns if there are MANY filename duplicates
    fileName = fileName[:-len(str(appendInt)) - 2] + f"({str(appendInt + 1)})"
    appendInt += 1
  fileName = f"{fileName}{ext}"

  try:
    r = requests.get(url)
  except Exception as e:
    print("Unable to download resource, see below")
    print(str(e))
    os.chdir("../")
    return
  try:
    with open(fileName, "wb") as f:
      f.write(r.content)
  except Exception as e:
    print(f"Unable to write to {fileName} in directory {os.getcwd()}")
    print(str(e))
    os.chdir("../")
  finally:
    os.chdir("../")
  print(f"file: {fileName}")


def back():
  global location
  global document
  global options
  global stateStack
  global forwardStateStack
  if len(
      stateStack
  ) > 1:  #go back if the previous state does not equal the current state.
    forwardStateStack.push(stateStack.pop())
    newStuff = stateStack.peek()
    location = newStuff[0]
    document = newStuff[1]
    options = newStuff[2]
    print(options.actionString)
  else:
    print("Cannot go back any farther")


def forward():
  global location
  global document
  global options
  global stateStack
  global forwardStateStack

  if not forwardStateStack.empty():
    stateStack.push(forwardStateStack.pop())
    newStuff = stateStack.peek()
    location = newStuff[0]
    document = newStuff[1]
    options = newStuff[2]
    print(options.actionString)
  else:
    print("Cannot go forward any farther")


def doRange():
  global options
  numOptions = len(options)
  min = -1
  while not (1 <= min and min <= numOptions):
    try:
      min = int(input("Start with action: "))
    except Exception as e:
      print(str(e))
      print("Try again")
      continue
    if not ((1 <= min and min <= numOptions)):
      print("Selection out of range")
      continue
  max = -1
  while not (1 <= max and max <= numOptions):
    try:
      max = int(input("End with action: "))
    except Exception as e:
      print(str(e))
      print("Try again")
      continue
    if not ((1 <= max and max <= numOptions)):
      print("Selection out of range")
      continue
  optionsMissed = list()
  for i in range(min, max + 1):
    if str(i) not in options:
      optionsMissed.append(str(i))
      continue
    if len(options[str(i)]) > 1:
      options[str(i)][0](*options[str(i)][1:])
    else:
      options[str(i)][0]()
  if len(optionsMissed) > 0:
    print("Unable to perform the following actions: ")
    for x in optionsMissed:
      print(f"{x}", end=(", " if not x == optionsMissed[-1] else ""))
    print()


def zipResources(dirName="images"):
  dir = os.listdir()
  if dirName not in dir:
    print(f"{dirName} not found in {os.getcwd()}")
    return
  if dirName + ".zip" in dir:
    appendInt = 1
    dirName = f"{dirName}({str(appendInt)})"
    while dirName + ".zip" in dir:
      dirName = f"{dirName[:-len(str(appendInt)) - 2]}({str(appendInt + 1)})"
      appendInt += 1
  try:
    shutil.make_archive(dirName, "zip", dirName)
  except Exception as e:
    print(f"Unable to zip {dirName}")
    print(str(e))
    return


""" Helper Functions """


def updateState(localLocation, localDocument, localOptions):
  """
    Function to introduce new pages to the state stack. 
    """
  global location
  global document
  global options
  global stateStack
  global forwardStateStack
  if (localLocation != location or localDocument != document
      or localOptions != options):
    location = localLocation
    document = localDocument
    options = localOptions
    forwardStateStack = Stack()
    stateStack.push((localLocation, localDocument, localOptions))


def fileExtensionOf(src):
  allowedFileTypes = {
      ".gif", ".jpeg", ".webm", ".png", ".svg", ".pdf", ".html", ".htm",
      ".txt", ".xls", ".xlsx", ".doc", ".docx", ".mp4", ".mp3", ".ogg", ".avi",
      ".jpg", ".mov", ".heic", ".avchd", ".ppt", ".pptx", ".m4a", ".wav"
  }

  indexOfLast = -1
  ext = ""
  for tipe in allowedFileTypes:
    index = src.rfind(tipe)
    if index > indexOfLast:
      indexOfLast = index
      ext = tipe
  return ext


def reformatLink(url):
  if url and url[0] == "/":
    return location + (url if url[-1] != "/" else url[:-1])
  else:
    return url


""" Debug Functions"""


def displayOptions(options):
  print(options.actionString)


def printState():
  global location
  global stateStack
  global forwardStateStack
  print(f"Location: {location}")
  print(f"State Stack has {len(stateStack)} states")
  print(f"Forward State Stack has {len(forwardStateStack)} states")


""" Main Function """


def main():
  """
    TEXT IN {} INDICATES POTENTIALLY UNRELIABLE INFORMATION

    The actions dictionary is a really strange way of doing the navigation system...
    each valid input corresponds to {a function pointer}

    If the command is not in actions, there is also a revolving door of options based on previous inputs to choose from. 
    These are formatted as follows: {each option contains a variable length tuple with the first element always being a function pointer. 
    The following elements are the arguments for the function. We can call these functions by unpacking these args}


    {This will probably be changed later, but for now, all functions called by actions (except exit), return options for the user}
    """
  #note: consider adding a flag system
  global location
  global document
  global options
  global stateStack

  #initialize state:
  location = "https://google.com"
  document = BeautifulSoup(requests.get(location).text, "html.parser")
  options = ActionDict()  #this isn't the ideal solution for initialization
  stateStack = Stack()
  stateStack.push((location, document, options))

  #command tables
  actions = ActionDict({
      "cd": cd,
      "ls": ls,
      "exit": sys.exit,
      "..": back,
      "forward": forward,
      "doRange": doRange,
      "zipResources": zipResources
  })  #always available
  debugActions = ActionDict({
      "displayOptions": (displayOptions, options),
      "printState": tuple([printState])
  })  #for debugging only
  actionEquivalencies = {
      "cd": "cd",
      "n": "cd",
      "new": "cd",
      "new page": "cd",
      "ls": "ls",
      "list links": "ls",
      "dir": "ls",
      "..": "..",
      "back": "..",
      "forward": "forward",
      "next": "forward",
      "exit": "exit",
      "doRange": "doRange",
      "zipResources": "zipResources"
  }  #allows command aliasing

  while (True):
    choice = input(f"{location[:45]}{'...' if len(location) > 45 else ''}> ")
    if choice in actionEquivalencies:
      actions[actionEquivalencies[choice]]()
    elif choice in options:
      #most entries in options should require arguments. This conditional is just to be safe
      if len(options[choice]) > 1:
        options[choice][0](*options[choice][1:])
      else:
        options[choice][0]()
    elif choice in debugActions:
      if len(debugActions[choice]) > 1:
        debugActions[choice][0](*debugActions[choice][1:])
      else:
        debugActions[choice][0]()
    else:
      print(f"No command \"{choice}\" found")
      continue


if __name__ == "__main__":
  main()
