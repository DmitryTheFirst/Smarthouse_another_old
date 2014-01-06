Smarthouse
==========

With this project you can control all electronic devices in your house. Main idea is extensibility. 
You can write your own classes and add them to Smarthouse.
Smarthouse is not only low-level control. 
For example, there are classes to control Respberry Pi through local network and classes to control video monitoring with motion sensor.
To perform such architecture, i'm using special class "Provider" which allows you to subscribe on events from other classes, create your own events or use typed methods from classes, using only their index
