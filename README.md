#Device Data Acquisition Service

##Windows Service for Parallel Measurement Device Communication, Data Acquisition, and Analysis
Powered by Gemstone.PQDIF

##📌 Overview

This project is a C# Windows Service designed to communicate with a large number of measurement devices in parallel, retrieve their data, store it in a SQL database, and run analytics on top of the collected data. The system supports FTP and custom TCP protocols, handles high-volume measurement data, performs 7-day sliding-window analytics, and exposes a WCF interface for remote management and monitoring.

All configuration values—such as SQL Server connection details—are loaded from a locally encrypted configuration file. Additional system configuration (devices, communication settings, preferred protocols, etc.) is retrieved from the SQL database during startup.
