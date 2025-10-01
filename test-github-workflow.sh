#!/bin/bash

# Script to test the GitHub Actions workflow
# This script will actually trigger the workflow and monitor its execution

echo "=== GitHub Actions Workflow Tester ==="
echo "This script will test the actual CI/CD workflow"
echo ""

# Check if GitHub CLI is available
if ! command -v gh &> /dev/null; then
    echo "❌ GitHub CLI (gh) is not installed"
    echo "Please install it: https://github.com/cli/cli#installation"
    exit 1
fi

# Check if we're authenticated
if ! gh auth status &> /dev/null; then
    echo "❌ Not authenticated with GitHub CLI"
    echo "Please run: gh auth login"
    exit 1
fi

echo "✅ GitHub CLI is available and authenticated"
echo ""

# Get repository information
REPO=$(gh repo view --json nameWithOwner -q .nameWithOwner)
echo "Repository: $REPO"
echo ""

echo "=== Testing Options ==="
echo "1. Test workflow validation (safe - no deployment)"
echo "2. Test main CI/CD workflow (will actually deploy!)"
echo "3. Check workflow status"
echo "4. View recent workflow runs"
echo ""

read -p "Choose an option (1-4): " choice

case $choice in
    1)
        echo ""
        echo "=== Running Workflow Validation Test ==="
        echo "This will run the validation workflow that tests configuration without deploying"
        echo ""
        
        # Trigger the test workflow
        echo "Triggering test workflow..."
        run_id=$(gh workflow run test-workflow.yml --json id -q .id 2>/dev/null)
        
        if [ $? -eq 0 ]; then
            echo "✅ Test workflow triggered successfully"
            echo "Run ID: $run_id"
            echo ""
            echo "Waiting for workflow to start..."
            sleep 5
            
            echo "Monitoring workflow execution..."
            gh run watch $run_id
        else
            echo "❌ Failed to trigger test workflow"
            echo "Make sure the test-workflow.yml file exists and is valid"
        fi
        ;;
        
    2)
        echo ""
        echo "⚠️  WARNING: This will trigger the actual CI/CD workflow!"
        echo "This will build, package, and deploy your application to Kubernetes"
        echo ""
        read -p "Are you sure you want to proceed? (y/N): " confirm
        
        if [[ $confirm =~ ^[Yy]$ ]]; then
            echo ""
            echo "=== Running Main CI/CD Workflow ==="
            echo "Triggering CI/CD pipeline..."
            
            # Trigger the main workflow
            run_id=$(gh workflow run ci-cd.yml --json id -q .id 2>/dev/null)
            
            if [ $? -eq 0 ]; then
                echo "✅ CI/CD workflow triggered successfully"
                echo "Run ID: $run_id"
                echo ""
                echo "Monitoring workflow execution..."
                gh run watch $run_id
            else
                echo "❌ Failed to trigger CI/CD workflow"
                echo "Check the workflow configuration and try again"
            fi
        else
            echo "Cancelled."
        fi
        ;;
        
    3)
        echo ""
        echo "=== Workflow Status ==="
        echo "Checking current workflow runs..."
        echo ""
        
        gh run list --limit 5 --json status,conclusion,workflowName,createdAt,url \
            --template '{{range .}}{{.workflowName}} - {{.status}} {{if .conclusion}}({{.conclusion}}){{end}} - {{timeago .createdAt}} - {{.url}}
{{end}}'
        ;;
        
    4)
        echo ""
        echo "=== Recent Workflow Runs ==="
        echo "Last 10 workflow runs:"
        echo ""
        
        gh run list --limit 10
        echo ""
        
        read -p "Enter a run ID to view details (or press Enter to skip): " run_id
        if [[ -n "$run_id" ]]; then
            echo ""
            echo "=== Workflow Run Details ==="
            gh run view $run_id
        fi
        ;;
        
    *)
        echo "Invalid option. Exiting."
        exit 1
        ;;
esac

echo ""
echo "=== Additional Commands ==="
echo "To manually trigger workflows:"
echo "  gh workflow run ci-cd.yml"
echo "  gh workflow run test-workflow.yml"
echo ""
echo "To watch a specific run:"
echo "  gh run watch <run-id>"
echo ""
echo "To view workflow logs:"
echo "  gh run view <run-id> --log"
echo ""
echo "=== End of Test Script ==="