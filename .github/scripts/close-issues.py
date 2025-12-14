#!/usr/bin/env python3
"""
GitHub Issue Closer for CI/CD Success
Closes open CI/CD failure issues when pipeline succeeds
"""

import os
import sys
import json
import requests
from datetime import datetime

# GitHub API configuration
GITHUB_TOKEN = os.environ.get('GITHUB_TOKEN')
GITHUB_REPOSITORY = os.environ.get('GITHUB_REPOSITORY')
GITHUB_RUN_ID = os.environ.get('GITHUB_RUN_ID')
GITHUB_RUN_NUMBER = os.environ.get('GITHUB_RUN_NUMBER')
GITHUB_SHA = os.environ.get('GITHUB_SHA')
GITHUB_WORKFLOW = os.environ.get('GITHUB_WORKFLOW')

# API endpoint
API_URL = f"https://api.github.com/repos/{GITHUB_REPOSITORY}"

# Headers for GitHub API
HEADERS = {
    "Authorization": f"token {GITHUB_TOKEN}",
    "Accept": "application/vnd.github.v3+json",
    "Content-Type": "application/json"
}

def find_open_ci_issues():
    """Find all open CI/CD failure issues"""
    try:
        search_url = f"{API_URL}/issues"
        # GitHub API accepts labels as comma-separated string in query params
        params = {
            "state": "open",
            "labels": "ci",
            "per_page": 100
        }
        
        response = requests.get(search_url, headers=HEADERS, params=params)
        response.raise_for_status()
        
        issues = response.json()
        
        # Filter for issues created by the bot (containing workflow reference)
        ci_issues = [
            issue for issue in issues 
            if "CI/CD Failure" in issue.get('title', '') or 
               "CI/CD Pipeline Failure" in issue.get('body', '')
        ]
        
        return ci_issues
    except Exception as e:
        print(f"Error finding open issues: {e}")
        return []

def close_issue(issue_number, issue_title):
    """Close an issue with a success comment"""
    try:
        workflow_url = f"https://github.com/{GITHUB_REPOSITORY}/actions/runs/{GITHUB_RUN_ID}"
        commit_url = f"https://github.com/{GITHUB_REPOSITORY}/commit/{GITHUB_SHA}"
        
        # Add comment explaining the closure
        comment = f"""## ✅ Pipeline Restored

The CI/CD pipeline has **succeeded** in a subsequent run.

### Success Details
- **Workflow**: {GITHUB_WORKFLOW}
- **Run Number**: #{GITHUB_RUN_NUMBER}
- **Commit**: [`{GITHUB_SHA[:7]}`]({commit_url})
- **Timestamp**: {datetime.utcnow().strftime('%Y-%m-%d %H:%M:%S UTC')}

### Links
- [Successful Workflow Run]({workflow_url})

---
*This issue was automatically closed by the CI/CD Failure Analysis Agent.*
"""
        
        # Post comment
        comment_data = {"body": comment}
        requests.post(
            f"{API_URL}/issues/{issue_number}/comments",
            headers=HEADERS,
            data=json.dumps(comment_data)
        )
        
        # Close the issue
        close_data = {"state": "closed"}
        response = requests.patch(
            f"{API_URL}/issues/{issue_number}",
            headers=HEADERS,
            data=json.dumps(close_data)
        )
        response.raise_for_status()
        
        print(f"✓ Closed issue #{issue_number}: {issue_title}")
        return True
        
    except Exception as e:
        print(f"✗ Failed to close issue #{issue_number}: {e}")
        return False

def main():
    if not GITHUB_TOKEN:
        print("Error: GITHUB_TOKEN not found in environment")
        sys.exit(1)
    
    if not GITHUB_REPOSITORY:
        print("Error: GITHUB_REPOSITORY not found in environment")
        sys.exit(1)
    
    print("Checking for open CI/CD failure issues to close...")
    
    open_issues = find_open_ci_issues()
    
    if not open_issues:
        print("No open CI/CD failure issues found")
        return
    
    print(f"Found {len(open_issues)} open CI/CD failure issue(s)")
    
    closed_count = 0
    for issue in open_issues:
        issue_number = issue['number']
        issue_title = issue['title']
        
        if close_issue(issue_number, issue_title):
            closed_count += 1
    
    print(f"\nClosed {closed_count} of {len(open_issues)} issue(s)")

if __name__ == "__main__":
    main()
