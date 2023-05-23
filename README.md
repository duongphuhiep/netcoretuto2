
UPDATE: After using clean architecture for many microservice over the past few years, I don't want to use it again.

"Clean architecture codes and folder organisation" seem clean with folders like "model", "infrastructure", "application" give a good impression for new-comer, and appeared to be easy to maintain.
But it is not.. it makes codes over complex, with many useless abstraction and mappings... I don't have good time with "Clean architect". 

In my case, Vertical Slice is the right option for me:

* Implement the feature you need with direct + straight forward codes as much as possible: from View you can call directly the Database abstraction.
* Make enough abstraction (or layer) so that you are able to mock and "fat-unit-test" the feature: You should only mock I/O codes or external library result.
* Avoid sharing codes between features. We should not worry too much about code duplicate until the last moment / refactor.

# Clean architect experiment

To reflect on this [Microsoft's excellent article](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/microservice-application-layer-implementation-web-api)

I made various experiment on 

* MediatR (on going)
* DI (Builit-in & Autofac) / Scrutor
* AutoMapper (TODO)
