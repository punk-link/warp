# Warp

Warp is a service where you can share short-lived texts and media with your friends and colleagues and be sure the data will be removed whenever you expect. The solution based on Asp.Net Core service and backed with KeyDB as a storage for sensitive user data.


## How to Run


### Environment Variables

To run the service you have to pass the following environment variables:

|Variable        |Description                        |
|----------------|-----------------------------------|
|PNKL_VAULT_ADDR |An address of a Vault instance     |
|PNKL_VAULT_TOKEN|An access token of a Vault instance|

### Docker Compose

Run the compose file inside the root directory. It sets up external dependencies like a database etc.


### Other

:exclamation: You may use _appSettings.Local.json_ for local runs.

Also you might need to override DB settings to run locally. Set up the _Redis_ section of your _appSettings.Local.json_.


## Styles

The project uses Sass to build its styles. To compile the styles:
1. Install Sass `npm install --global sass`
2. Install Yarn `npm install --global yarn`
3. Go to the root project folder and update project dependencies `yarn upgrade`
4. Go to _Warp.WebApp/Styles_ folder and execute the following command `sass --style compressed main.scss ../wwwroot/css/main.min.css`


## Project Icons

The project uses these [icofont](https://icofont.com) glyphs:

- align-left
- bin
- clock
- close
- exclamation-tringle
- eye
- hill-sunny
- link
- loop
- pencil-alt-2
- plus
- simple-left
- worried