Authors: Alex Merrill, Scott Powell, Mary Scott

## Instructions

First, you need to clone this repository to your machine.

To play our game and utilize the computer vision aspect, you need to have python installed. If you do not wish to use Python. The game will run, however the heart rate will not update and the game will be in permanent calm mode.

You are also going to need some python libraries.
To download the libraries easily, you can run this command from the 701gameProject directory (You must have pip installed for this to work):

```bash
pip install -r requirements.txt
```
(Note: tensorflow does not appear to work with pip. While the above command should work, it may not. Tensorflow may need to be installed by other means)

Next, you need to run the one of the two computer vision scripts. hr_with_emotions.py contains both stress and emotion detection, while hr.py contains just stress detection. Try running hr_with_emotions.py first, however if you have compatability or performance issues you should run hr.py instead. To run the script, run this command from the 701gameProject directory:

```bash
python hr_with_emotions.py
# or
python hr.py
```

(If you would like to see the video, you may add the -v option. If you do not have a camera but want to validate that the python script works, you can use -i /path/to/video/of/face)

The script takes a few seconds to establish a resting heart rate, but may be started before running the game. Once you have the script running, you can open up the build, "Final Game", in 701gameProject.
