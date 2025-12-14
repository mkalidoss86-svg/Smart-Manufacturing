#!/bin/bash

set -e

# CI/CD Failure Analysis Script
# This script analyzes pipeline failures and creates GitHub issues automatically

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "========================================="
echo "CI/CD Failure Analysis Agent"
echo "========================================="

# Check if all stages passed
ALL_PASSED=true
FAILED_STAGES=()
FAILURE_DETAILS=""

# Analyze each stage
check_stage() {
    local stage_name=$1
    local stage_status=$2
    
    echo "Checking stage: $stage_name - Status: $stage_status"
    
    if [[ "$stage_status" == "failure" ]]; then
        ALL_PASSED=false
        FAILED_STAGES+=("$stage_name")
        echo -e "${RED}✗ $stage_name FAILED${NC}"
    elif [[ "$stage_status" == "success" ]]; then
        echo -e "${GREEN}✓ $stage_name PASSED${NC}"
    elif [[ "$stage_status" == "skipped" ]]; then
        echo -e "${YELLOW}⊘ $stage_name SKIPPED${NC}"
    fi
}

# Check all stages
check_stage "Build" "$BUILD_STATUS"
check_stage "Test" "$TEST_STATUS"
check_stage "Docker Build" "$DOCKER_STATUS"
check_stage "Docker Compose" "$COMPOSE_STATUS"
check_stage "Kubernetes Deploy" "$K8S_STATUS"

# If all passed, close any open issues for this workflow
if [ "$ALL_PASSED" = true ]; then
    echo -e "${GREEN}All stages passed successfully!${NC}"
    echo "Checking for open failure issues to close..."
    
    # Close open issues
    python3 .github/scripts/close-issues.py
    
    exit 0
fi

# If failures detected, create issue
echo -e "${RED}Pipeline failures detected: ${FAILED_STAGES[*]}${NC}"
echo "Creating GitHub issue for failures..."

# Call Python script to create detailed issue
if python3 .github/scripts/create-issue.py "${FAILED_STAGES[@]}"; then
    echo "Issue creation completed successfully"
else
    echo "ERROR: Failed to create issue" >&2
    exit 1
fi

exit 0
