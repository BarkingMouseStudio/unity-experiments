Setup
===

In the simulation, a background noise of 3 Hz is used in both input and output layers in order to weaken the synaptic weights between uncorrelated neurons.

The network encodes these angles after discretizing them into bins with 5o resolution.

The directions encoded are also discretized using a 45o resolution,
resulting in 26 possible movements of the end-effector from its original
position.

Training period
---

0. EXCLUSIVE: Select set of "home" (XXX: starting?) positions.
1. EXCLUSIVE: ERG produces random motor commands between [-5o, 5o].
2. EXCLUSIVE: These random motor commands are applied at each joint.
3. EXCLUSIVE: (Effects on the spatial position of the end-effector is computed based on forward kinematic equations. This does not require the IK rig.)
3.5. EXCLUSIVE: Each iteration includes four movements of the arm, and for each movement, the neurosimulation encodes the joint positions, the spatial direction of movement and the motor commands with a firing stimulus of 20 msec in the respective neuronal layers.
  - An interval of 50 msec is set between the firing patterns that represent two consecutive movements.
	- In the simulation, a background noise of 3 Hz is used in both input and output layers in order to weaken the synaptic weights between uncorrelated neurons.
4. EXCLUSIVE: The ERG output randomly stimulates motor neurons. (Could alternatively randomly stimulate, via Set, the output layer and Get to produce the motor command. This has the benefit of reducing the difference between the two periods.)
5. Proprioception of the current joint angles is encoded into the first 4 layers of the network.
6. Desired spatial direction of the end effector is encoded into the next 3 layers of the network.

Performance period
---

1. Proprioception of the current joint angles is encoded into the first 4 layers of the network.
2. Next spatial direction of the end effector is encoded into the next 3 layers of the network.
3. EXCLUSIVE: Network produces motor commands in the form of velocity which drive the respective joints.


Each iteration includes four movements of the arm, and for each movement, the neurosimulation encodes the _joint positions_, the _spatial direction of movement_ and the _motor commands_ with a firing stimulus of 20 msec in the respective neuronal layers. An interval of 50 msec is set between the firing patterns that represent two consecutive movements.

Action is generated through the Endogenous Random Generator (ERG) [2], which sends random motor commands, and the results of these actions in the spatial domain are perceived and associated.

During the period of the action-perception cycle, proprioception stimulates sensory neurons and encodes into the network the current joint configuration.

The endogenous random generator (ERG) randomly stimulates motor neurons.

The resulting joint commands move the end-effector in a certain spatial direction which is observed and encoded into the network.

The temporal correlation between neuronal firings is extracted by the Spike Timing-Dependent Plasticity mechanism which modifies the synaptic weights so that if the robotic arm is at the pose just learned and the end-effector should move in the same spatial direction, then the motor neurons that were originally activated by ERG, should now be stimulated by the current they receive from the input neurons. Currently, the spatial position of the end-effector in the training stage is computed by solving forward kinematic equations.

The Endogenous Random Generator sends random motor commands, in the range of [−5o, 5o] at each joint, and their effect on the spatial position of the end-effector is computed based on forward kinematic equations.
