# Getting Started with Skyline DataMiner DevOps

Welcome to the Skyline DataMiner DevOps environment!  
This quick-start guide will help you get up and running.  
For more details and comprehensive instructions, please visit [DataMiner Docs](https://docs.dataminer.services/).

## Creating a DataMiner Application Package

This project is configured to create a `.dmapp` file every time you build the project.  
When you compile or build the project, you will find the generated `.dmapp` in the standard output folder, typically the `bin` folder of your project.

When you publish the project, a corresponding item will be created in the online DataMiner Catalog.

## The DataMiner Package Project

This project is designed to create multi-artifact packages in a straightforward manner.

### Adding Extra Artifacts in the Same Solution

You can right-click the solution and select **Add** and then **New Project**. This will allow you to select DataMiner project templates (e.g. adding additional Automation scripts).

> [!NOTE]
> Connectors are currently not supported.

Every **Skyline.DataMiner.SDK** project, except other DataMiner package projects, will by default be included within the `.dmapp` created by this project.  
You can customize this behavior using the **PackageContent/ProjectReferences.xml** file. This allows you to add filters to include or exclude projects as needed.

<!-- Currently not supported
### Adding Content from the Catalog

You can reference and include additional content from the Catalog using the **PackageContent/CatalogReferences.xml** file provided in this project.
 -->

### Importing from DataMiner

You can import specific items directly from a DataMiner Agent:  

1. Connect to an Agent via **Extensions > DIS > DMA > Connect**.

1. If your Agent is not listed, add it by going to **Extensions > DIS > Settings** and clicking **Add** on the DMA tab.

1. Once connected, you can import specific DataMiner artifacts: in your **Solution Explorer**, navigate to folders such as **PackageContent/Dashboards** or **PackageContent/LowCodeApps**, right-click, select **Add**, and select **Import DataMiner Dashboard/Low Code App** or the equivalent.

## Executing Additional Code on Installation

Open the **Kata Swarming.cs** file to write custom installation code. Common actions include creating elements, services, or views.

**Quick tip:** Type `clGetDms` in the `.cs` file and press **Tab** twice to insert a snippet that gives you access to the **IDms** classes, making DataMiner manipulation easier.

## Does Your Installation Code Need Configuration Files?

You can add configuration files (e.g. `.json`, `.xml`) to the **SetupContent** folder, which can be accessed during installation.

Access them in your code using:

```csharp
string setupContentPath = installer.GetSetupContentDirectory();
```


## Publishing to the Catalog

This project was created with support for publishing to the DataMiner Catalog.  
You can publish your artifact manually through Visual Studio or by setting up a CI/CD workflow.
## Publishing to the Catalog with Complete CI/CD Workflow

This project includes a comprehensive GitHub workflow that adheres to Skyline Communications' quality standards, including static code analysis, custom validation, and unit testing.

### Prerequisite

You need a **SonarCloud Organization**. If you don’t have one, you can create it [here](https://sonarcloud.io/create-organization).

### Steps

1. Create a GitHub repository by going to **Git > Create Git Repository**, selecting GitHub, and filling in the wizard before clicking **Create and Push**.

1. In GitHub, go to the *Actions* tab.

1. Click the workflow run that failed (usually called *Add project files*).

1. Click the "build" step that failed and read the failing error.

   ``` text
   Error: DATAMINER_TOKEN is not set. Release not possible!
   Please create or re-use an admin.dataminer.services token by visiting: https://admin.dataminer.services/.
   Navigate to the right Organization then go to Keys and create/find a key with permissions to Register Catalog Items.
   Copy the value of the token.
   Then set a DATAMINER_TOKEN secret in your repository settings: **Dynamic Link**
   ```

   You can use the links from the actual error to better address the next couple of steps.

1. Obtain an **Organization Key** from [admin.dataminer.services](https://admin.dataminer.services/) with the following scopes:
   - **Register Catalog items**
   - **Read Catalog items**

1. Add the key as a secret in your GitHub repository, by navigating to **Settings > Secrets and variables > Actions** and creating secrets or variables with the required names.

1. Re-run the workflow.

The following secrets and variables will have been added to your repository after all issues are resolved:

| Name            | Type    | Description                                        | Setup Guide                                                                                 |
|-----------------|---------|----------------------------------------------------|---------------------------------------------------------------------------------------------|
| `DATAMINER_TOKEN` | Secret  | Organization key for publishing to the Catalog   | Obtain from [admin.dataminer.services](https://admin.dataminer.services/) and add it as a secret. |
| `SONAR_TOKEN`    | Secret  | Token for SonarCloud authentication               | Obtain from [SonarCloud Security](https://sonarcloud.io/account/security) and add it as a secret.  |
| `SONAR_NAME`     | Variable | SonarCloud project ID                            | Visit [SonarCloud](https://sonarcloud.io/projects/create), copy the project ID, and add it as a variable. |

### Releasing a Version

1. Navigate to the **<> Code** tab in your GitHub repository.

1. In the menu on the right, select **Releases**.

1. Create a new release, select the desired version as a **tag**, and provide a title and description.

> [!NOTE]
> The description will be visible in the DataMiner Catalog.
