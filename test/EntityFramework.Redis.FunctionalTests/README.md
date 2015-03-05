Redis.FunctionalTests
=====
## Configuration
These tests require a redis-server to be running.
If you want to start your own then set the Environment Variable STARTED_OWN_REDIS_SERVER. You are then responsible for starting and stopping your own Redis server on port 6379.
Otherwise the test will start one for you (in this case ensure you have run build.cmd to pull down the Redis package and ensure there is no pre-existing Redis server running on port 6379).
```