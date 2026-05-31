Welcome to Serveron (r) TM View Standard

This file is intended to help you properly install the software. A complete set
of release notes also accompanies this file. The release notes can be found on
the distribution CD-ROM and are also installed with the software.


1. Files included on the distribution CD


Documents
	Folder containing documentation. You can find there the following files 
	in each language folder:
		Readme.txt - this file.
		FAQ.rtf - Frequently Asked Questions
		Installation Manual.pdf - a short manual guiding the user through
			installation options
		ReleaseNotes.rtf - what is particular to this release of TM View
		TM View Software Manual.pdf - the reference manual for TM View.

TM View Database
	This folder contains the SQL Server Express installation files with all
	prerequisites as well as the install script to create the TM View database

TM View
	Folder containing the installation files for the TM View application and all
	prerequisites.

SQL Scripts
	Folder containing all the scripts for creating and updating the database used
	by TM View. This folder is used by the Database installer.

Images
	Folder containing images for logos and splash screens.


Setup.hta
	Local Web page presenting installation options.	


2. Minimum System Requirements

Serveron TM View has different requirements for the Viewer
and the Server installation models.
NOTE: These are the minimum requirements for installing and running TM View.
They do not provide the best experience using TM View.
The requirements for a more pleasing user experience are described
in the installation manual.

The TM View Server installation requires a computer with 1.5 GHz or higher processor and
at least 1 GB memory or higher.
Multi-core (Duo-core or quad-core) processors are highly recommended

The TM View Viewer installation requires a computer with 1 GHz processor and 
at least 500 Mb memory.
A recent (post-2005) display card with a 1024x768 or higher screen resolution is highly
recommended. Visual anomalies may occur on an 800x600 display. 256 or more colors are required.
Serveron recommends use of the highest color mode (color quality) supported by your
video adapter or subsystem.  This may be called "True Color," "High Color,"
"24-bit color," or "32-bit color" depending on the manufacturer.

TM View, together with its prerequisite software, may require up to 300MB of
disk space to install.  The actual space required for normal operations may
be significantly less. 
At least 50MB of additional space should be available for normal use.
The computer containing the SQL Server anddatabse nmust have enough disk space for the data to
grow: in average, the database will grow by 2.5 MB per monitor, per year with 4 hours sampling,
and a verification run every 3 days.
Serveron TM View is a Microsoft(tm) Windows(r)-based application. This product
works with Windows XP (SP3), Windows Server 2003 (SP2), Windows 7 and Windows Server 2008
operating systems. 32-bits and 64-bits (x64 only) operating systems are also
supported. Windows 98, Windows ME, Windows NT and Windows 2000 are not supported.
Microsoft Windows Vista(r) is not supported in this release.

Serveron TM View requires the Microsoft .NET Framework version 3.5.

3. Installing the Software

IMPORTANT: Administrative privilege is required to install the software.

The installation program will check for the presence of .NET Framework version
3.5. This prerequisite is required to run the application.

The installation also requires the Visual Studio 2008 redistributables
from Microsoft(tm). This software will be automatically installed.

You will be asked to choose one of the two (2) installation configurations
(Server or Viewer).  For more information, please see
section 5, "Choose an Installation Configuration," below in this file.

When installation is complete, you may start TM View using either the Start
menu or the Desktop icon.  You may open the User's Manual from the Help menu
and follow the introductory sections to familiarize yourself with TM View.

Serveron recommends that you use Windows Update to check for any recent updates
to Microsoft components after installing TM View on your computer.

4. Upgrading from a previous release of TM View

IMPORTANT: Administrative privilege is required to upgrade the software.

This installation of TM View will remove any previous installation of TM View (2.1 or 3.x)
and replace it with this new version of TM View. All the transformers and TM3/TM8 monitors will be
migrated to the new TM View version if the previous installation of TM View is 3.0 or newer. 
Note: This version of TM View does not support TrueGas monitors.

Refer to the Installation section.

5. Choosing an Installation Configuration

Serveron TM View may be installed in any one of two (2) configurations:
Viewer or Server. TM View requires that the Server configuration be
installed at least once in the whole system. If more than one person
needs to access the data collected by TM View, the successive installations after
the Server installation must be Viewer installations.

TM View is made up of three major components:
- The Database
- The Poller
- The Viewer

The Viewer is the application you see and interact with.  The Poller works
"behind the scenes," communicating with your Serveron monitors and saving
the data they gather in the database.

The database is the central repository for all the configuration data for TM View 
and for all the data collected from your Serveron Monitors. It is an
essential part of the TM View system. 
The TM View installation CD contains an installer for Microsoft SQL Server Express 2008.
You can install this component on any computer that satisfies the hardware 
requirements for the Server. Any TM View installation relying on this default SQL
Server installation will be limited to 1 Server and up to 14 clients (Viewer installations).

Alternately, you may decide to provide your own Microsoft SQL Server instance. It can be
SQL Server 2005, SQL Server 2005 Express, SQL Server 2008 or SQL Server 2008 Express.
The installer for TM View will prompt you to provide the connection string to the SQL
Server instance you chose. The installation CD contains the SQL scripts necessary to create
the database as required by TM View.

The Poller is the central point of communication with your Serveron monitors. 
The Poller will connect to all your monitors and collect their data, then store it in
the database.
The Poller is only installed in the Server configuration, which needs to be installed
on a computer which will operate '24x7'and is expected to have ongoing 
access to the necessary communications resources (modems, network adapters, etc.)
A Server installation can receive monitor data when no user is logged in.
In Server Installations, both the Poller and the Viewer component are installed.
This instance of the Viewer is the only one in which configuration changes in the system
can be initiated (adding monitors, transformers, etc.) and in which direct communication
with a monitor can be carried out (changing limits, etc.).

In the Viewer configuration, as the name implies, the Poller component
is not installed.  Viewer installations are limited to accessing data
retrieved and stored in the database by the Poller in the Server installation. This
configuration is appropriate for workstations where the users need to see
the transformer data but do not have the need or the ability to communicate
directly with Serveron monitors from their computer.


To install a Server configuration, you must use a Windows logon ID which has
administrative access to the database you will use. This is because the installer will
execute several SQL scripts to create and populate the database with the objects
required for TM View operations.


6. Accessing the Release Notes

Serveron Release Notes include information about last-minute changes and
program behaviors. The Release Notes may be reached from the Help menu
(Help: Documentation: ReleaseNotes).

For more information, contact your nearest BPL Global Representative or BPL Global, Ltd.

BPL Global, Ltd
20325 NW Von Neumann Drive
Suite 120
Beaverton, OR 97006
Phone: (503) 924-3200
Toll-free: (800) 880-2552 (USA and Canada only)
Fax: (503) 924-3290

http://www.bplglobal.net

 * * * * * * * * * * *

Serveron is a registered trademark of BPL Global, Ltd.

Microsoft, Microsoft .NET, Windows, Visual Studio 2005 and Visual Studio 2008
are either registered trademarks or trademarks of Microsoft Corporation
in the United States and/or other countries.
 

Copyright (c) 2011 BPL Global, Ltd. All rights reserved.

