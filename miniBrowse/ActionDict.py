""" 
Extends dictionary and defines how to print objects 
Also forces keys to be string only
"""
from collections import defaultdict


class ActionDict(dict):
    def __init__(self, mapping=None, /,  **kwargs):
        if mapping is not None:
            mapping = {str(key):value for key, value in mapping.items()}
            super().__init__(mapping)
        else:
            mapping = None
        if kwargs:
            mapping.update( {str(key):value for key, value in kwargs.items()} )
        self.actionString = ""
    
    def __index__(self, key):
        if str(key) in self:
            super().__index__(str(key))
        else:
            return None

    def __setitem__(self, key, value):
        key = str(key)
        super().__setitem__(key, value)

    def __eq__(self, other):
        if (self.actionString != other.actionString):
            return False
        if(not super().__eq__(other)):
            return False
        return True
    def __ne__(self, other):
        return not self.__eq__(other)
    
    def update(self, mapping=None, /, **kwargs):
        super().update({str(key):value for key, value in mapping.items()}, **kwargs)

class Stack:
    def __init__(self):
        self.__index = []

    def __len__(self):
        return len(self.__index)

    def push(self,item):
        self.__index.insert(0,item)

    def peek(self):
        if len(self) == 0:
            raise Exception("peek() called on empty stack.")
        return self.__index[0]

    def pop(self):
        if len(self) == 0:
            raise Exception("pop() called on empty stack.")
        return self.__index.pop(0)

    def __str__(self):
        return str(self.__index)
    
    def empty(self):
        if self.__len__() == 0:
            return True
        else:
            return False
