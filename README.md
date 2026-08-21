# TechBox
Open Source IT Technician box.

# Requirements

- Windows 11
- DotNet 10.0 Desktop
- A computer on a domain

# Features

[] Read user informations from Domain
[] Backup/Restore user profile
[] Read computer informations
[] Execute CCM actions on remote computer
[] Remote computer throught RDP or CCM Remote Viewer
[] Remote power off or reboot computer

# TODO

[] Allow extensions
[] Integrate GLPI

# Getting Started

The application lives in `src/TechBox` (WPF, `net10.0-windows`).

```
dotnet build TechBox.sln
dotnet run --project src/TechBox
```

On first launch (or whenever no LDAP location has been saved yet), TechBox opens a
mandatory window listing the domain's LDAP tree so the user can pick an OU/container
to use as the default location. The application does not proceed until a location has
been selected and confirmed. The selection can later be changed from
`Fichier > Changer l'emplacement LDAP...` in the main window.