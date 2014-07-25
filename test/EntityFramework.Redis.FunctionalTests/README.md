Redis.FunctionalTests
=====
These tests require a path to a local Redis Server.

## Configuration
To configure these tests, either start your own redis server locally on the default port of 6379 or provide the path to the redis-server.exe in a config file. If both we cannot find an existing service and we cannot start the specified server then **all tests will be skipped** rather than failing.

#### Config File
Create a file called `RedisTest.config` in user's %USERPROFILE% directory. Add a valid server path. You can also add a time after which the server will be stopped (default = 10 secs). See example below.

Example:
```xml
<config>
    <RedisServer>
        <Path>C:\work\bin\Redis</Path>
        <TimeoutInSecs>5</TimeoutInSecs>
    </RedisServer>
</config>
```