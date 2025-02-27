using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GitHubActionsTrigger
{
    class Program
    {
        private static readonly string GitHubApiUrl = "https://api.github.com";
        private static readonly string Owner = Environment.GetEnvironmentVariable("GITHUB_OWNER");    // Refer your GitHub Account
        private static readonly string Repo = Environment.GetEnvironmentVariable("GITHUB_REPO");  // Refer repo with your workflows 
        private static readonly string WorkflowId = Environment.GetEnvironmentVariable("GITHUB_WORKFLOW_ID"); // Refer workflow to call
        private static readonly string PersonalAccessToken = Environment.GetEnvironmentVariable("GITHUB_PAT");  // Refer your Personal Access Token (PAT).
        private static readonly string File = "samples_files/Program.txt"; // Your "Program" file to be processed by workflow. Samples file on folder samples_files

        static async Task Main(string[] args)
        {
            await UploadAndCommitFile(File, "AI Sanbox: Add Program.txt file to build and test demostration.");
            await TriggerGitHubActionsWorkflow();
        }


        private static async Task UploadAndCommitFile(string filePath, string commitMessage)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(GitHubApiUrl);
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", PersonalAccessToken);

                var fileContent = System.IO.File.ReadAllText(filePath);
                var base64Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileContent));

                Console.WriteLine("Generating code powered by AI Sandbox");

                await Task.Delay(5000); // 5 seconds


                var requestBody = new
                {
                    message = commitMessage,
                    content = base64Content,
                    branch = "main" // Branch to commit to
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var url = $"/repos/{Owner}/{Repo}/contents/samples_files/{System.IO.Path.GetFileName(filePath)}";
                var response = await client.PutAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Code generated sucessfully ...");
                    await Task.Delay(3000); // 3 seconds
                    Console.WriteLine("Code uploaded and committed successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to upload and commit file. Status code: {response.StatusCode}");
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response: {responseContent}");
                }
            }
        }

        private static async Task TriggerGitHubActionsWorkflow()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(GitHubApiUrl);
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", PersonalAccessToken);

                var requestBody = new
                {
                    @ref = "main" // Branch to run the workflow on
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var url = $"/repos/{Owner}/{Repo}/actions/workflows/{WorkflowId}/dispatches";
                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Workflow triggered successfully.");
                    await GetWorkflowRunResults(client);
                }
                else
                {
                    Console.WriteLine($"Failed to trigger workflow. Status code: {response.StatusCode}");
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response: {responseContent}");
                }
            }
        }

        private static async Task GetWorkflowRunResults(HttpClient client)
        {
            var url = $"/repos/{Owner}/{Repo}/actions/runs";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                //await Task.Delay(5000); // 5 seconds

                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic workflowRuns = JsonConvert.DeserializeObject(responseContent);

                // Wait for workflow starting
                await Task.Delay(5000); // 5 seconds

                // Poll the status of the latest run until it completes
                while (true)
                {
                    response = await client.GetAsync(url);
                    responseContent = await response.Content.ReadAsStringAsync();
                    workflowRuns = JsonConvert.DeserializeObject(responseContent);

                        if (workflowRuns.workflow_runs[0].conclusion == "success") //success
                        {
                          
                        Console.WriteLine("Workflow run completed.");
                        Console.WriteLine("The code generated by AI Sandbox successfully compiled and passed unit tests");
                        Console.WriteLine($"Code generation, compilation, and testing finished with workflow process ID: {workflowRuns.workflow_runs[0].id}");

                        return;
                        
                        }

                        if (workflowRuns.workflow_runs[0].conclusion == "cancelled") //cancelled
                        {
                          
                        Console.WriteLine("Workflow was cancelled.");
                        Console.WriteLine($"Code generation, compilation, and testing finished with workflow process ID: {workflowRuns.workflow_runs[0].id}");

                        return;
                        
                        }

                        if (workflowRuns.workflow_runs[0].conclusion == "failure") //failure
                        {
                          
                        Console.WriteLine("Workflow failed.");
                        Console.WriteLine("The code generated by AI Sandbox failere to be compiled");
                        Console.WriteLine($"Code generation, compilation, and testing finished with workflow process ID: {workflowRuns.workflow_runs[0].id}");

                        return;
                        
                        }

                    Console.WriteLine("Waiting for workflow run to complete...");
                    Console.WriteLine("Waiting for 5 seconds before to call again ...");
                    Console.WriteLine($"Timestamp: {DateTime.Now}");
                    Console.WriteLine(".... _______________Begin_____________________ ...");

                    await Task.Delay(5000); // 5 seconds

                    response = await client.GetAsync(url);
                    responseContent = await response.Content.ReadAsStringAsync();
                    workflowRuns = JsonConvert.DeserializeObject(responseContent);

                    Console.WriteLine($"Last Run ID 3: {workflowRuns.workflow_runs[0].id}");
                    
                    Console.WriteLine($"Run ID: {workflowRuns.workflow_runs[0].id}, Status: {workflowRuns.workflow_runs[0].status}, Conclusion: {workflowRuns.workflow_runs[0].conclusion}");

                    Console.WriteLine(".... ______________Finish______________________ ...");

                    var latestRunId = workflowRuns.workflow_runs[0].id;
                    Console.WriteLine($"Latest Run ID: {latestRunId}");

                }

            }
            else
            {
                Console.WriteLine($"Failed to get workflow runs. Status code: {response.StatusCode}");
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response: {responseContent}");
            }
        }
    }
}