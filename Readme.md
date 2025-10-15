_Intro_

The Kodak Pakon has a SDK/API called TLX installed on the machine. This is a COM-server which contains all the low level functions needed for using a Pakon through software. At some point, there probably was some documentation distributed for using this as a client, and part of that documentation an example application came along, known as the TLXClientDemo. The UI is very bare bones because the Pakon developers just shoved exactly everything the COM-server was capable of doing in there, with a checkbox or radio button for every setting.

Windows COM is old and forgotten but was pretty cool for its age. It is actually possible to do remote COM calls from another machine. And during the early 2000s the Pakon team probably thought that film scanners would have a long future yet, little did they know.. The SDK was probably not very popular among commercial users, just getting started was a bit too much and the payoff was that you could build your own software for scanning film. At some point the Pakon team saw that Microsoft .NET was gaining some serious traction, and being a programming language that is faster to get started with, they built their own interop layer between C# and the COM-server. A C# lib was probably also distributed at some point, luckily the internal Pakon Troubleshooting tool uses this lib and so it can be found as the Pakon.dll in the PTS directory.

Through a disassembler it is possible to reverse engineer the Pakon.dll and get some well needed clues as to how to work with a Pakon through software. And through the reverse engineering software [Ghidra](https://ghidra-sre.org/) it is possible to gain some insights into TLX.dll.

_What we know_
- TLA, TLB, TLC, TLX are COM DLL:s that TLXClientDemo interfaces with. Before TLXClientDemo there was a TLAClientDemo. There is a reference manual for TLA in the docs folder.
- TLXClientDemo is a demonstration app that you can run the Pakon without using the official software
- Pakon.dll is a .NET library for using TLX that can be found in the PTS folder. This can be quite easily reverse engineered with for example [ILSpy](https://github.com/icsharpcode/ILSpy)
- But development for Windows XP is tricky today, best to run an old version of VS2010 on the same virtual machine as the Pakon is running on

_How far have I come?_

It is possible to initialize TLX and run commands towards the TLX API. It is currently possible to try it out to fetch your model information and serial number.  A simple console client is included which can now perform a basic scan of a colour negative roll and save the result as JPEG images. This is just a proof-of-concept to show that it is indeed possible to talk to your Pakon scanner with home built software.

_Possibilites maybe?_

- If the drivers and COM-server can be installed on a newer machine, it would be possible to run a Pakon on something more modern. Maybe not Windows 11 but even 7 would be an upgrade
- Remote COM-calls from another machine, have the pakon machine to run headless
- Streamlined client for getting raw scans, together with [my raw converter](https://github.com/eatfrog/pakonrawconverter) you could reduce the number of clicks a lot to get a good nice 16 bit scans

_Important links_

[Kai Kaufman](https://ktkaufman03.github.io/blog/2022/09/04/pakon-reverse-engineering/) has made important progress on reverse engineering the pakon driver and have gotten them to work on 64bit Windows.
Check the source for his driver [here](https://github.com/ktkaufman03/FX35).

_How to build_

There is something weird going on with msvcp71.dll and msvcr71.dll. It needs to be in the folder of the executable and also the F-X35 COM SERVER folder. Probably it is in system32 on a winxp machine but missing on anything more modern? I am not sure what is going on here. If you are seeing a PartitionAlreadySelected error it is most likely due to this. You have logs in C:\Program Files (x86)\Pakon\F-X35 COM SERVER that you can check for more info.