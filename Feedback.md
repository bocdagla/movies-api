### Feedback

*Please add below any feedback you want to send to the team*

First of all thanks a lot for the opportunitty 

1. You will notice that I have used the classic Controller, Service, Repository pattern to mount this simple API, 
the reason is because there was no apparent need of a event driven architecture nor a CQRS it looked overkill to use command/handler architecture
(Also I was dealing with entitties and not bounded contexts therefore no sign of DDD)
2. The Grpc calls to your server NEVER fail, while making some tests I tried it with 500 calls (manually) and all of them went ok.
3. You mentioned in your readme.md that "We know that [**Provided API**](http://localhost:7172/swagger/index.html) may have some configuration issues, and we will like them to be found and fixed, if possible."
I wasn't able to understand what was missing, maybe the movies.json? I found the file amovies.json that needed to be renamed to movies.json i Just need to renamed it overriding the entrypoint of the docker-compose
because creating a new Dockerfile and adding the command then again the entrypoint would have been too much for the little that was needed.
4. The server starts a on a swagger, either way i have added the curls into the cUrl.txt
5. I was Completely unable to test the mock Grpc services, moq is treating the async calls as sync ones making it nearly impossible to test it

And thats it! Very fun test and I would love to upload this test to my Github given the permission to do so :)
