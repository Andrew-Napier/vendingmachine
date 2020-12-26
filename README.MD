Vending Machine - Back End
==========================

Overview
--------

This project uses Azure Functions Serverless architecture to provide an interface to a vending machine.
It can be debugged locally, using HTTP requests to perform the following features:

| Function | Verb | URL |
|----------|------|-----|
| add-payment | [POST] |  http://localhost:7071/api/add-payment |
| products | [GET] | http://localhost:7071/api/products |
| purchase | [POST] | http://localhost:7071/api/purchase |
| final-purchase | [GET] | http://localhost:7071/api/final-purchase |

All requests require a header parameter of "x-id", to identify the user making the purchase.
(In a real service, OAuth would provide a bearer-token with a "user-id" property in the JWT payload, but this 
was ommitted for simplicity).

- The **products** end point returns a list of items, including their id, description, quantity in stock and price.
- To commence "shopping", call the **add-payment** endpoint, specifying a query-parameter of **money=1.23** to specify the amount added to the vending machine.
- Use the **purchase** endpoint to buy items that are in-stock, using a query-parameter of **item=id** where (id is derived from the data returned in the **products** request)
- User the **final-purchase** endpoint to receive a JSON object representing the shopping-cart, and showing the "remaining funds" after purchases.

Technical notes and implementation details.
-------------------------------------------
- There is no user-interface provided in this solution.
- The **HttpTriggers.cs** file is the "top-level" of an Azure Functions app.  This is the entry point for HTTP/HTTPS requests.
- I have included a Postman Collection file that exercises the end-points. (See https://postman.com) 
- For simplicity sake, the Products list has been hard-coded.  Obviously, in a real world solution the `ProductsDictionary` would be database backed.
- The `ShoppingCartFactory` would likewise be backed by storage to avoid memory pressures that this would face should there be a massive spike in users.
- As mentioned above, a header variable was used as "authentication".  This is not secure and open to abuse.
- I think I missed a chance to use a CQRS pattern with the object returned in the final-purchase API.  - Remaining stock-levels are unlikely to be of interest to the consumer
- Typically of a test I tried to "dive too deep" and the unit tests suffered in terms of breadth and depth of coverage.
- My definition of a "unit test" is as written.  Each test addresses only one class.  An "integration test" would define multiple real "subject-under-test" (sut) classes to ensure they behaved together the way it would be expected to. 
- I wasn't happy with the amount of technical verification I originally had going on in the triggers, so introduced the `HttpRequestHandler` service to extract this from other logic.
- There isn't as much care to servicing simultaneous requests as I would like.  For instance, I wanted to use a lot more immutable objects to "update" stock-levels 

_there's always more that can be done_


