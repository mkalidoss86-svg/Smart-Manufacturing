# CI/CD Failure Analysis Agent

## Overview

The CI/CD Failure Analysis Agent is an automated system that monitors pipeline execution and automatically creates GitHub Issues when failures occur. It provides detailed failure analysis, classification, and suggested remediation steps.

## Features

- **Automatic Failure Detection**: Monitors all pipeline stages (Build, Test, Docker, Docker Compose, Kubernetes)
- **Intelligent Classification**: Categorizes failures by type with appropriate labels
- **Detailed Issue Reports**: Creates comprehensive issues with:
  - Clear title indicating the failed stage
  - Failure summary with timeline
  - Relevant log snippets
  - Commit SHA and workflow run links
  - Root cause suggestions
  - Recommended next actions
- **Duplicate Prevention**: Avoids creating multiple issues for the same commit
- **Auto-Assignment**: Assigns issues to repository owner/maintainer
- **Auto-Closure**: Closes issues automatically when subsequent pipeline runs succeed
- **Proper Labeling**: Automatically adds labels (ci, build, docker, k8s, test)

## Architecture

### Pipeline Stages

1. **Build**: Compiles and builds the application
2. **Test**: Runs automated tests
3. **Docker Build**: Builds container images
4. **Docker Compose**: Tests multi-container setup
5. **Kubernetes Deploy**: Deploys to Kubernetes cluster

### Components

#### 1. CI Pipeline Workflow (`.github/workflows/ci-pipeline.yml`)
- Defines all pipeline stages
- Runs on push/PR to main and develop branches
- Collects stage statuses
- Triggers failure analysis

#### 2. Failure Analysis Script (`.github/scripts/failure-analysis.sh`)
- Analyzes stage results
- Determines if failures occurred
- Orchestrates issue creation or closure

#### 3. Issue Creator (`.github/scripts/create-issue.py`)
- Creates detailed GitHub issues
- Fetches logs from failed jobs
- Classifies failures
- Provides remediation suggestions
- Prevents duplicate issues

#### 4. Issue Closer (`.github/scripts/close-issues.py`)
- Finds open CI failure issues
- Closes them when pipeline succeeds
- Adds success comment with details

## Failure Classification

| Failure Type | Labels | Typical Causes |
|--------------|--------|----------------|
| Build | ci, build, bug | Syntax errors, missing dependencies, compilation issues |
| Test | ci, test, bug | Failing tests, broken functionality, test environment issues |
| Docker Build | ci, docker, bug | Dockerfile errors, missing base images, context issues |
| Docker Compose | ci, docker, docker-compose, bug | Service configuration, port conflicts, volume issues |
| Kubernetes | ci, k8s, kubernetes, bug | Manifest errors, cluster issues, resource constraints |

## Issue Format

Issues created by the agent include:

```markdown
## CI/CD Pipeline Failure Report

### Summary
The CI/CD pipeline failed during the **[Stage Name]** stage.

### Failure Details
- Failed Stage(s): [list]
- Workflow: [workflow name]
- Run Number: #[number]
- Commit: [SHA with link]
- Branch: [branch name]
- Triggered by: @[username]
- Timestamp: [UTC timestamp]

### Links
- Workflow Run
- Commit Details

### Stage Status
[Table of all stages and their statuses]

### Log Snippets
[Relevant logs from failed jobs]

### Possible Root Causes
[List of potential causes]

### Recommended Actions
[Numbered list of next steps]
```

## Root Cause Analysis

The agent provides intelligent suggestions based on the failure stage:

### Build Failures
- Check for syntax errors
- Verify dependencies
- Ensure build tools are updated
- Review recent code changes

### Test Failures
- Review test logs
- Check if changes broke functionality
- Verify test environment
- Check for race conditions

### Docker Failures
- Verify Dockerfile syntax
- Check base image accessibility
- Ensure paths are correct
- Review build context

### Docker Compose Failures
- Check docker-compose.yml syntax
- Verify service dependencies
- Ensure ports are available
- Check volume mounts

### Kubernetes Failures
- Verify manifest syntax
- Check cluster connectivity
- Ensure resources are available
- Review service configurations

## Usage

The agent runs automatically on every push or pull request to main/develop branches. No manual intervention is required.

### Viewing Issues

1. Go to the repository's Issues tab
2. Filter by labels: `ci`, `build`, `docker`, `k8s`, `test`
3. Review failure details and follow recommended actions

### Manual Testing

To test failure scenarios, you can modify the pipeline to simulate failures:

```yaml
- name: Simulate Build Failure
  run: exit 1
```

## Requirements

- GitHub Actions enabled
- `GITHUB_TOKEN` with issues write permission (automatically provided)
- Python 3.x in runner environment
- Bash shell

## Permissions

The workflow requires the following permissions:
- `issues: write` - To create and close issues
- `actions: read` - To read workflow run details and logs
- `contents: read` - To checkout code

## Customization

### Adjust Stage Detection

Edit `.github/scripts/failure-analysis.sh` to modify which stages are monitored.

### Customize Issue Templates

Edit `.github/scripts/create-issue.py` to modify issue format and content.

### Change Labels

Update the `classify_failure()` function in `create-issue.py` to change labels.

### Modify Root Cause Suggestions

Edit `get_root_cause_suggestions()` and `get_next_actions()` functions.

## Troubleshooting

### Issues Not Being Created

1. Check workflow run logs
2. Verify GITHUB_TOKEN has issues permission
3. Ensure scripts are executable (`chmod +x`)
4. Check Python script outputs in workflow logs

### Duplicate Issues

The agent prevents duplicates by checking commit SHA. If duplicates occur:
- Check `check_duplicate_issue()` function logic
- Verify issue search is working correctly

### Issues Not Auto-Closing

1. Verify all stages passed
2. Check `close-issues.py` is being called
3. Ensure issue search criteria match created issues

## Best Practices

1. **Keep it Updated**: Regularly review and update root cause suggestions
2. **Monitor False Positives**: Adjust classification logic if needed
3. **Review Closed Issues**: Learn from patterns in failures
4. **Customize for Your Stack**: Adapt suggestions to your technology stack

## Future Enhancements

- Integration with Slack/Teams for notifications
- ML-based root cause prediction
- Historical failure trend analysis
- Automated retry mechanism for transient failures
- Integration with monitoring/observability tools

## License

This component is part of the Smart Manufacturing Quality Platform.
