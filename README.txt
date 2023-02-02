Load the Unity scene to start
Press the Spacebar to start the simulation

The AI uses compute shaders to speed up matrix calculations, so if you don't have a GPU
it will probably cause some issues / significant speed losses.

PROJECT SETTINGS
fixed timestep = 0.01 - the AI runs 100 times per second (mainly for stability of physics)
physics.autosimulation = false - the AI runs the physics

BOOTSTRAP (unity object)
----------------------------
the networks are stored in 4 text files: mu, sigma, value and running. mu and sigma store
the actor's neural network, while value stores the critic's neural network. Running
stores some moving averages that make the training more efficient

TESTING - true (shows the AI running), false (actually trains the AI)

LOAD_ID - the name of the network you want to load. If it doesn't exist
          the app will create one

FOLDER_PATH - the path of the folder containing your neural networks

RUN_ID - the name of the AI, when new training sessions finish, the AI will be saved
         under this name with it's performance statistics along side it

COUNTER - keeps track of the current training session, you'll need to set this manually
          to the highest number if you start the application multiple times

NEXT_CHECKPOINT - milestone for timesteps, after this number is reached the AI is saved

CHECKPOINT_INTERVAL - how far apart should automatic saves be?

i've included several AI shown in my video that you can load up using the LOAD_ID
----------------------------

PHYSICS MANAGER (unity object)
----------------------------
the simulation is managed by this object

MODE - REALTIME (runs at normal speed), FAST (runs as fast as possible)

ENVIRONMENTS - a list of all the active environments, you can add more than one if you like

STEPS - how many steps does the FAST mode do at a time?
----------------------------
