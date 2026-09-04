# Image_Processing_Service_API

## Tech Stack
- C#
- .NET 8
- ASP.NET Core
- EF Core
- SQL Server
- RabbitMQ
- Redis
- SixLabors.ImageSharp

## Features
- JWT User Authentication and Authorization
- Use ImageSharp for Image Process including:<br>
  Resize, Crop, Rotate, Watermark, Flip, Mirror, Compress,<br>
  Change foramt (JPEG, PNG, etc), Apply filters (grayscale, sepia)
- Redis caching transformed images to improve performance
- Fixed window rate limiter to prevent abuse
- Global ExceptionHandler
- Implements a thread-safe, reusable RabbitMQ connection with SemaphoreSlim
- Use RabbitMQ offloading image process to background workers, reduce client waiting response time 

## Follow-up Project
https://roadmap.sh/projects/image-processing-service

For Six Labors license, please refer to https://licensing.sixlabors.com