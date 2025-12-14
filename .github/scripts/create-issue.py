#!/usr/bin/env python3
"""
GitHub Issue Creator for CI/CD Failures
Creates detailed issues for pipeline failures with proper classification and labels
"""

import os
import sys
import json
import requests
from datetime import datetime

# Constants
LOG_LINES_TO_EXTRACT = 30

# GitHub API configuration
GITHUB_TOKEN = os.environ.get('GITHUB_TOKEN')
GITHUB_REPOSITORY = os.environ.get('GITHUB_REPOSITORY')
GITHUB_RUN_ID = os.environ.get('GITHUB_RUN_ID')
GITHUB_RUN_NUMBER = os.environ.get('GITHUB_RUN_NUMBER')
GITHUB_SHA = os.environ.get('GITHUB_SHA')
GITHUB_REF = os.environ.get('GITHUB_REF')
GITHUB_ACTOR = os.environ.get('GITHUB_ACTOR')
GITHUB_WORKFLOW = os.environ.get('GITHUB_WORKFLOW')

# Stage statuses
BUILD_STATUS = os.environ.get('BUILD_STATUS')
TEST_STATUS = os.environ.get('TEST_STATUS')
DOCKER_STATUS = os.environ.get('DOCKER_STATUS')
COMPOSE_STATUS = os.environ.get('COMPOSE_STATUS')
K8S_STATUS = os.environ.get('K8S_STATUS')

# API endpoint
API_URL = f"https://api.github.com/repos/{GITHUB_REPOSITORY}"

# Headers for GitHub API
HEADERS = {
    "Authorization": f"token {GITHUB_TOKEN}",
    "Accept": "application/vnd.github.v3+json",
    "Content-Type": "application/json"
}

def classify_failure(stage_name):
    """Classify failure type and suggest labels"""
    stage_lower = stage_name.lower()
    
    if "build" in stage_lower and "docker" not in stage_lower:
        return "build", ["ci", "build", "bug"]
    elif "test" in stage_lower:
        return "test", ["ci", "test", "bug"]
    elif "docker" in stage_lower and "compose" not in stage_lower:
        return "docker", ["ci", "docker", "bug"]
    elif "compose" in stage_lower:
        return "docker-compose", ["ci", "docker", "docker-compose", "bug"]
    elif "kubernetes" in stage_lower or "k8s" in stage_lower:
        return "kubernetes", ["ci", "k8s", "kubernetes", "bug"]
    else:
        return "unknown", ["ci", "bug"]

def get_root_cause_suggestions(stage_name):
    """Provide root cause suggestions based on failure stage"""
    suggestions = {
        "build": [
            "Check for syntax errors in source code",
            "Verify all dependencies are correctly specified",
            "Ensure build tools and compilers are up to date",
            "Review recent code changes that might break compilation"
        ],
        "test": [
            "Review failing test logs for specific assertions",
            "Check if recent code changes broke existing functionality",
            "Verify test environment configuration",
            "Check for race conditions or timing issues"
        ],
        "docker": [
            "Verify Dockerfile syntax and instructions",
            "Check if base image is accessible",
            "Ensure all COPY/ADD paths are correct",
            "Review Docker build context and .dockerignore"
        ],
        "docker-compose": [
            "Check docker-compose.yml syntax",
            "Verify service dependencies and network configuration",
            "Ensure required ports are not already in use",
            "Check volume mounts and file permissions"
        ],
        "kubernetes": [
            "Verify Kubernetes manifests syntax",
            "Check cluster connectivity and permissions",
            "Ensure required resources are available",
            "Review service and ingress configurations"
        ]
    }
    
    failure_type, _ = classify_failure(stage_name)
    return suggestions.get(failure_type, ["Review logs for specific error messages"])

def get_next_actions(stage_name):
    """Suggest next actions based on failure stage"""
    actions = {
        "build": [
            "Pull latest changes and build locally",
            "Check build logs in the workflow run",
            "Verify dependency versions match requirements"
        ],
        "test": [
            "Run failing tests locally with verbose output",
            "Check if tests pass on main branch",
            "Review test coverage and recent changes"
        ],
        "docker": [
            "Build Docker image locally to reproduce",
            "Verify Dockerfile on main branch",
            "Check Docker Hub for base image availability"
        ],
        "docker-compose": [
            "Run docker-compose locally to reproduce",
            "Check service logs for startup errors",
            "Verify environment variables and secrets"
        ],
        "kubernetes": [
            "Check cluster status and resource availability",
            "Verify kubectl access and permissions",
            "Review deployment logs and pod status"
        ]
    }
    
    failure_type, _ = classify_failure(stage_name)
    return actions.get(failure_type, ["Review workflow logs", "Contact DevOps team"])

def check_duplicate_issue(commit_sha):
    """Check if an issue already exists for this commit"""
    try:
        # Search for open issues with the commit SHA
        search_url = f"{API_URL}/issues"
        params = {
            "state": "open",
            "labels": "ci",
            "per_page": 100
        }
        
        response = requests.get(search_url, headers=HEADERS, params=params)
        response.raise_for_status()
        
        issues = response.json()
        for issue in issues:
            # Check if issue body contains the commit SHA
            if commit_sha in issue.get('body', ''):
                print(f"Found duplicate issue: #{issue['number']}")
                return issue['number']
        
        return None
    except Exception as e:
        print(f"Error checking for duplicates: {e}")
        return None

def fetch_job_logs(failed_stages):
    """Fetch relevant job logs from the workflow run"""
    try:
        jobs_url = f"{API_URL}/actions/runs/{GITHUB_RUN_ID}/jobs"
        response = requests.get(jobs_url, headers=HEADERS)
        response.raise_for_status()
        
        jobs = response.json().get('jobs', [])
        log_snippets = {}
        
        for job in jobs:
            job_name = job.get('name', '')
            if any(stage.lower() in job_name.lower() for stage in failed_stages):
                # Get job logs
                logs_url = f"{API_URL}/actions/jobs/{job['id']}/logs"
                log_response = requests.get(logs_url, headers=HEADERS)
                
                if log_response.status_code == 200:
                    # Extract last N lines of logs
                    log_lines = log_response.text.split('\n')
                    last_lines = log_lines[-LOG_LINES_TO_EXTRACT:] if len(log_lines) > LOG_LINES_TO_EXTRACT else log_lines
                    log_snippets[job_name] = '\n'.join(last_lines)
        
        return log_snippets
    except Exception as e:
        print(f"Warning: Could not fetch job logs: {e}")
        return {}

def create_issue(failed_stages):
    """Create a GitHub issue for the pipeline failure"""
    
    # Check for duplicate
    duplicate_issue = check_duplicate_issue(GITHUB_SHA[:7])
    if duplicate_issue:
        print(f"Duplicate issue #{duplicate_issue} already exists for commit {GITHUB_SHA[:7]}")
        print("Skipping issue creation to avoid duplicates")
        return
    
    # Determine primary failure stage (first one that failed)
    primary_stage = failed_stages[0]
    failure_type, labels = classify_failure(primary_stage)
    
    # Create issue title
    title = f"CI/CD Failure: {primary_stage} Failed in Run #{GITHUB_RUN_NUMBER}"
    
    # Fetch logs for failed stages
    log_snippets = fetch_job_logs(failed_stages)
    
    # Create detailed issue body
    workflow_url = f"https://github.com/{GITHUB_REPOSITORY}/actions/runs/{GITHUB_RUN_ID}"
    commit_url = f"https://github.com/{GITHUB_REPOSITORY}/commit/{GITHUB_SHA}"
    branch_name = GITHUB_REF.replace('refs/heads/', '').replace('refs/tags/', '')
    
    body = f"""## CI/CD Pipeline Failure Report

### Summary
The CI/CD pipeline failed during the **{primary_stage}** stage.

### Failure Details
- **Failed Stage(s)**: {', '.join(failed_stages)}
- **Workflow**: {GITHUB_WORKFLOW}
- **Run Number**: #{GITHUB_RUN_NUMBER}
- **Commit**: [`{GITHUB_SHA[:7]}`]({commit_url})
- **Branch**: {branch_name}
- **Triggered by**: @{GITHUB_ACTOR}
- **Timestamp**: {datetime.utcnow().strftime('%Y-%m-%d %H:%M:%S UTC')}

### Links
- [Workflow Run]({workflow_url})
- [Commit Details]({commit_url})

### Stage Status
| Stage | Status |
|-------|--------|
| Build | {BUILD_STATUS} |
| Test | {TEST_STATUS} |
| Docker Build | {DOCKER_STATUS} |
| Docker Compose | {COMPOSE_STATUS} |
| Kubernetes Deploy | {K8S_STATUS} |

"""
    
    # Add log snippets
    if log_snippets:
        body += "### Log Snippets\n\n"
        for job_name, logs in log_snippets.items():
            body += f"#### {job_name}\n"
            body += "```\n"
            body += logs
            body += "\n```\n\n"
    else:
        body += f"### Logs\nView detailed logs in the [workflow run]({workflow_url}).\n\n"
    
    # Add root cause suggestions
    body += "### Possible Root Causes\n"
    suggestions = get_root_cause_suggestions(primary_stage)
    for suggestion in suggestions:
        body += f"- {suggestion}\n"
    body += "\n"
    
    # Add next actions
    body += "### Recommended Actions\n"
    actions = get_next_actions(primary_stage)
    for action in actions:
        body += f"1. {action}\n"
    body += "\n"
    
    body += "---\n"
    body += "*This issue was automatically created by the CI/CD Failure Analysis Agent.*\n"
    body += "*It will be automatically closed when a subsequent pipeline run succeeds.*"
    
    # Create the issue
    issue_data = {
        "title": title,
        "body": body,
        "labels": labels
    }
    
    try:
        response = requests.post(
            f"{API_URL}/issues",
            headers=HEADERS,
            data=json.dumps(issue_data)
        )
        response.raise_for_status()
        
        issue = response.json()
        issue_number = issue['number']
        issue_url = issue['html_url']
        
        print(f"✓ Successfully created issue #{issue_number}")
        print(f"  URL: {issue_url}")
        
        # Try to assign to repository owner or maintainer
        try:
            owner = GITHUB_REPOSITORY.split('/')[0]
            assign_data = {"assignees": [owner]}
            requests.post(
                f"{API_URL}/issues/{issue_number}/assignees",
                headers=HEADERS,
                data=json.dumps(assign_data)
            )
            print(f"✓ Assigned issue to @{owner}")
        except Exception as e:
            print(f"Warning: Could not assign issue: {e}")
        
    except requests.exceptions.HTTPError as e:
        print(f"✗ Failed to create issue: {e}")
        print(f"  Response: {e.response.text}")
        sys.exit(1)
    except Exception as e:
        print(f"✗ Unexpected error creating issue: {e}")
        sys.exit(1)

def main():
    if not GITHUB_TOKEN:
        print("Error: GITHUB_TOKEN not found in environment")
        sys.exit(1)
    
    if not GITHUB_REPOSITORY:
        print("Error: GITHUB_REPOSITORY not found in environment")
        sys.exit(1)
    
    # Get failed stages from arguments
    failed_stages = sys.argv[1:]
    
    if not failed_stages:
        print("No failed stages provided")
        sys.exit(0)
    
    print(f"Creating issue for failed stages: {', '.join(failed_stages)}")
    create_issue(failed_stages)

if __name__ == "__main__":
    main()
