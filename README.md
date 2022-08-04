# TeamVaultCli
Command Line Tool to send / receive credentials and credential details to a [TeamVault](https://github.com/seibert-media/teamvault) installation
uses [Fody](https://github.com/Fody/Fody) to package the complete tool into one .exe for easy deployment.

Still in an early development stage. Feel free to contribute :-)

## Usage

    --api                TeamVault Api adress e.g. https://MyOwnTeamVault.MyDomain.com/
    --tvu                Required. username for authenticating against TeamVault Api
    --tvp                password for authenticating against TeamVault Api
    --tvps               use password in the windows credential store or ask for it
    -s, --send           send update of given secret to TeamVault
    -n, --name           name-field of the secret in TeamVault
    -u, --username       username-field of the secret in TeamVault
    -p, --password       password-field of the secret in TeamVault
    --url                url-field of the secret in TeamVault
    -d, --description    description-field of the secret in TeamVault e.g. 5Dlkg5
    -t, --template       secret id of a template when adding new entries - security settings will be copied
    -r, --receive        receive password of a credential and return it
    --help               Display this help screen.
    --version            Display version information.

## Example

Upsert Sercret to your Teamvault installation:

    TeamVaultRest.exe --api="https://MyOwnTeamVault.MyDomain.xyz/" --tvu=build --tvp=[service user password] -s --name="teamvault api | new credential" --url="http://newapplication.de" -d"new stuff is great :D" -p"need a password also"

Retrieve password from vault

    TeamVaultRest.exe --api="https://MyOwnTeamVault.MyDomain.xyz/" --tvu=build --tvp=[service user password] -r


## defaults.json
Baked into the code is a "defaults.json" where you can define some standard values for you're environment, here an example:

    {
        "TeamVaultUrl": "https://MyOwnTeamVault.MyDomain.xyz/",
        "allowed_users": [ "UsernameA", "UsernameB" ],
        "allowed_groups": [ "Role-Teamvault-DevTeam" ]
    }

## Further Notes
The implementation is quite experimental and uses the Rest-API but also the Web-Backend (as the Rest-API does not provide all the needed functionality)

## Main Usecase
The tool was created to automatically upload credentials, urls, ... to TeamVault in scope of a build process where infrastructure is created automatically including passwords/credentials/url/... 
TeamVault is good for managing and sharing this credentials in a manual way - now automation is also possible!