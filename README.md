# Warp

Warp is a service where you can share short-lived texts and media with your friends and colleagues and be sure the data will be removed whenever you expect. The solution based on Asp.Net Core service and backed with KeyDB as a storage for sensitive user data.


## How to Run

To run the service you have to pass the following environment variables:

|Variable        |Description                        |
|----------------|-----------------------------------|
|PNKL_VAULT_ADDR |An address of a Vault instance     |
|PNKL_VAULT_TOKEN|An access token of a Vault instance|

:exclamation: You may use _appSettings.Local.json_ for local runs.

Also you might need to override DB settings to run locally. Set up the _Redis_ section of your _appSettings.Local.json_.