
CC = gcc
CFLAGS = -Wall 

# Source file and executable name
SOURCE = rush.c
TARGET = rush

all: $(TARGET)

$(TARGET): $(SOURCE)
	$(CC) $(CFLAGS) -o $(TARGET) $(SOURCE)

# Clean rule
clean:
	rm -f $(TARGET)