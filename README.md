# Mass-Edit Permissions
Uses the Services API to edit group permissions across all workspaces. Right now, it only removes the "Add Document" permission. 

## How to use
In the same folder/directory as the .exe, include a file named `groups.txt` that contains the Artifact IDs of the groups whose permissions you'd like to modify.

Optionally, you can include a file named `workspaces.txt` that contains the Artifact IDs of the workspaces for those groups. 

Logs will be outputted to `log.txt` in the same directory.
