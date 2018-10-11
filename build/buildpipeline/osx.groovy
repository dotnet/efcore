@Library('dotnet-ci') _

simpleNode('OSX10.12','latest') {
    stage ('Checking out source') {
        checkout scm
    }
    try {
        stage ('Build') {
            sh './build.sh --ci'
        }
    }
    finally {
        archiveArtifacts allowEmptyArchive: true, artifacts: "artifacts/**/*", onlyIfSuccessful: false
        xunit {
            mstest pattern:"artifacts/**/*.trx", skipIfNoTestFiles: true
        }
    }
}
