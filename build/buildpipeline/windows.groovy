@Library('dotnet-ci') _

// 'node' indicates to Jenkins that the enclosed block runs on a node that matches
// the label 'windows-with-vs'
simpleNode('Windows_NT','latest') {
    stage ('Checking out source') {
        checkout scm
    }
    try {
        stage ('Build') {
            def environment = 'set Test__SqlServer__DefaultConnection: Server=(local)\\SQL2016;Database=master;User ID=sa;Password=Password12! & set Test__SqlServer__SupportsMemoryOptimized: true'
            bat "${environment} & .\\build.cmd -ci /bl:artifacts/logs/msbuild.binlog"
        }
    }
    finally {
        archiveArtifacts allowEmptyArchive: true, artifacts: "artifacts/**/*", onlyIfSuccessful: false
        archiveXUnit {
            mstest pattern:"artifacts/**/*.trx", skipIfNoTestFiles: true
        }
    }
}
