# TDTResultsScanner
Small helper program that scans results from Jenkins server

# Use

Just run it. It will pull the latest build from the jenkins server.

1. Build. This build can take a while because it could have to download a full build of ROOT.
2. Exit VS and use the created .bat file to start Visual studio (getting ROOT environment variable correct)
3. Use the windows credential manager to add your login credentials for the build server.
3. Run it

# Development

Clone & Build. VS should download everything. Note that you will get warnings until you restart
VS with the .bat file that is made by the first time you build.
