# Project Overview

Warp is a service that allows users to share short-lived texts and media with friends and colleagues, ensuring data is automatically removed after a specified time. The platform combines an ASP.NET Core backend with a Vue 3 single-page application and relies on KeyDB for managing sensitive user data.


## Technologies

- ASP.NET Core for backend services (`Warp.WebApp`, `Warp.Measurements`, `Warp.KeyManager`)
- Vue 3 + TypeScript + Vite for the SPA located under `Warp.ClientApp`
- KeyDB (Redis fork) for data storage
- Docker and Docker Compose for containerization and local orchestration
- .NET source generators for logging constants, domain errors, and messages (`Warp.CodeGen`)
- Tailwind CSS and Sass for styling