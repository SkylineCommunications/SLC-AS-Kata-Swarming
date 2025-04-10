# Tutorial - Getting started with Swarming

Complementary package for the [Getting started with Swarming tutorial](https://aka.dataminer.services/GettingStartedWithSwarmingTutorial).

Follow this tutorial along with an instructional video in [Kata #61: Getting started with Swarming](https://community.dataminer.services/courses/kata-61/).

## Overview

The main content of this package is the *Enable Swarming* automation script which is an interactive script that will guide you through the process of enabling the Swarming feature.

![Script](./Images/Swarming_Tutorial_Enable_No_Problems.png)

It will check off all the hard prerequisites and then so some automatic analysis of your automation scripts and QActions to see if any incompatible AlarmID usage is detected.

If everything checks out, you can give your DMS the command to enable it cluster wide.
Note that this involves a full DMS restart.

## Prerequisites

To deploy this integration from the Catalog, you’ll need:

- DataMiner version 10.5.1+/10.6.0+
- A DataMiner System connected to dataminer.services
