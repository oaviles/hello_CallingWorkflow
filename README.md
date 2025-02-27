# Calling GitHub Workflows

This C# code is part of a console application designed to interact with GitHub's API. The application is defined within the GitHubActionsTrigger namespace. This app uses important configuration values, such as the GitHub API URL, repository owner, repository name, workflow ID, and personal access token. These values are retrieved from environment variables, ensuring that sensitive information is not hardcoded into the source code.

These environment variables are used to call a GitHub Workflow to trigger it and wait for finish workflow execution, this app is monitoring workflows status every 5 seconds.


### Related to [Bot Coder PoC](https://github.com/oaviles/hello_BotCoder) 
### Related to [Bouncy Castle PoC](https://github.com/oaviles/hello_Bouncy-Castle) 