# Warp


## How to Run

To run the service you have to pass the following environment variables:

|Variable        |Description                        |
|----------------|-----------------------------------|
|PNKL_VAULT_ADDR |An address of a Vault instance     |
|PNKL_VAULT_TOKEN|An access token of a Vault instance|

:exclamation: You may use _appSettings.Local.json_ for local runs.

Also you might need to override DB settings to run locally. Set up the _Redis_ section of your _appSettings.Local.json_.