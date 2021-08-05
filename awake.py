"""
Please run this script from Comand prompt
"""
import pyautogui
import time
import sys
from datetime import datetime
pyautogui.FAILSAFE = False
numMin = None
print("Press CTrl+C to terminate while statement")
if ((len(sys.argv)<2) or sys.argv[1].isalpha() or int(sys.argv[1])<1):
    numMin = 1
else:
    numMin = int(sys.argv[1])

try:
    while(True):
        x=0
        while(x<numMin):
            time.sleep(10)
            x+=1
        for i in range(0,200):
            pyautogui.moveTo(0,i*4)
        pyautogui.moveTo(1,1)
        for i in range(0,3):
            pyautogui.press("shift")
        print("Movement made at {}".format(datetime.now().time()))
except KeyboardInterrupt:
    print("Done")