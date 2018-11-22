# Checkout.API
[![Build Status](https://travis-ci.org/PierreRoudaut/checkout-api.svg?branch=master)](https://travis-ci.org/PierreRoudaut/checkout-api)

## About
Checkout.API simulates a real-time marketplace by exposing a RESTful API that allows customers to list a catalog of products, add/remove items to a cart, update their quantity according to the stock availble, as well as clearing out the cart entirely.

A set of back-office routes allows administrators to perform CRUD operations on the product catalog

Any operation that alters the state of a shopping cart  _(e.g adding or removing an item to cart)_ will trigger a push notification by the server (using SignalR) that will notify every connected clients with the newly updated available quantity for the given product.

On top of that, any iddle client that does not perform operations on their cart will trigger the cart the be cleared, updating therefore the available quantity of the products. Connected clients will be notified accordingly

## Development Environment

__Prerequesites__: Checkout API is powered by [ASP.NET Core 2.1](https://www.microsoft.com/net/download) and [SQL Server](https://www.microsoft.com/en-us/sql-server/). Those two main dependecies should be installed locally.

1. Clone the repo
2. Run the following commands:
```bash
cd Checkout.API
cp 'appsettings.json' 'appsettings.Development.json'
## Replace the default connection string with the desired one
dotnet restore
dotnet ef database update
dotnet build
dotnet run
```
3. Open a browser and navigate to _http://{host}:{port}/api/swagger_


## API Documentation

A [static documentation](swagger.md) of the API can be found here

Otherwise, the API is fully self-documented by swagger and is accessible at _/api/swagger_ 

## Continuous Integration

Checkout.API uses [Travis CI](https://travis-ci.org/) to run every tests in the test project __Checkout.API.Test__

## Testing

```bash
cd Checkout.API.Tests
dotnet test
```

## TODO

 - Support realtime products update for administration page
 - Add authentication
 - Setup continuous deployment script
 - Support checkout