# EGM-Core-Module
A simple C# console-based game update system that supports:

Installing update packages via CLI

Manifest + version validation

Pre-install script execution (simulated)

Automatic rollback on failure

Version tracking

Install history logging

 Project Overview

This project simulates a game update workflow similar to modern patching systems.

Core Features

 Validate update package existence
Validate manifest + version
Execute pre-install hook
Rollback on failure
Maintain version state
Maintain install history log
 Clear step-by-step logging


If pre-install succeeds:

Copy files into /game_versions/{version}

Update current_version

Update last_known_good_version

Update current symlink (or pointer file)

