#!/bin/bash

# =============================================================================
# Deployment Script - Event-Driven Bookstore Notification System
# Usage: ./deploy.sh <environment> [--what-if]
# =============================================================================

set -e  # Exit on error

# -----------------------------------------------------------------------------
# Configuration
# -----------------------------------------------------------------------------

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
INFRA_DIR="$(dirname "$SCRIPT_DIR")/infra"
VALID_ENVIRONMENTS=("dev" "test" "stg" "prod")
DEPLOYMENT_LOCATION="eastus"

# -----------------------------------------------------------------------------
# Colors for output
# -----------------------------------------------------------------------------

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# -----------------------------------------------------------------------------
# Functions
# -----------------------------------------------------------------------------

print_header() {
    echo -e "${BLUE}"
    echo "=============================================="
    echo "  Bookstore Notification System - Deployment"
    echo "=============================================="
    echo -e "${NC}"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ $1${NC}"
}

show_usage() {
    echo "Usage: $0 <environment> [options]"
    echo ""
    echo "Environments:"
    echo "  dev     Development environment"
    echo "  test    Test/QA environment"
    echo "  stg     Staging environment"
    echo "  prod    Production environment"
    echo ""
    echo "Options:"
    echo "  --what-if    Preview changes without deploying"
    echo "  --help       Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 dev                 Deploy to development"
    echo "  $0 prod --what-if      Preview production deployment"
}

validate_environment() {
    local env=$1
    for valid_env in "${VALID_ENVIRONMENTS[@]}"; do
        if [[ "$env" == "$valid_env" ]]; then
            return 0
        fi
    done
    return 1
}

check_prerequisites() {
    print_info "Checking prerequisites..."

    # Check Azure CLI
    if ! command -v az &> /dev/null; then
        print_error "Azure CLI is not installed. Please install it first."
        exit 1
    fi
    print_success "Azure CLI found"

    # Check Bicep
    if ! az bicep version &> /dev/null; then
        print_warning "Bicep CLI not found. Installing..."
        az bicep install
    fi
    print_success "Bicep CLI found"

    # Check Azure login
    if ! az account show &> /dev/null; then
        print_error "Not logged into Azure. Please run 'az login' first."
        exit 1
    fi
    print_success "Azure CLI logged in"

    # Display current subscription
    local subscription=$(az account show --query name -o tsv)
    print_info "Current subscription: $subscription"
}

validate_bicep() {
    print_info "Validating Bicep templates..."

    if ! az bicep build --file "$INFRA_DIR/main.bicep" --stdout > /dev/null 2>&1; then
        print_error "Bicep validation failed"
        az bicep build --file "$INFRA_DIR/main.bicep"
        exit 1
    fi
    print_success "Bicep templates are valid"
}

# -----------------------------------------------------------------------------
# Main Script
# -----------------------------------------------------------------------------

print_header

# Parse arguments
ENVIRONMENT=""
WHAT_IF=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --what-if)
            WHAT_IF=true
            shift
            ;;
        --help)
            show_usage
            exit 0
            ;;
        *)
            if [[ -z "$ENVIRONMENT" ]]; then
                ENVIRONMENT=$1
            else
                print_error "Unknown argument: $1"
                show_usage
                exit 1
            fi
            shift
            ;;
    esac
done

# Validate environment argument
if [[ -z "$ENVIRONMENT" ]]; then
    print_error "Environment is required"
    show_usage
    exit 1
fi

if ! validate_environment "$ENVIRONMENT"; then
    print_error "Invalid environment: $ENVIRONMENT"
    echo "Valid environments: ${VALID_ENVIRONMENTS[*]}"
    exit 1
fi

print_info "Environment: $ENVIRONMENT"
print_info "Location: $DEPLOYMENT_LOCATION"

# Check prerequisites
check_prerequisites

# Validate Bicep
validate_bicep

# Parameter file path
PARAM_FILE="$INFRA_DIR/parameters/$ENVIRONMENT.parameters.json"

if [[ ! -f "$PARAM_FILE" ]]; then
    print_error "Parameter file not found: $PARAM_FILE"
    exit 1
fi
print_success "Parameter file found: $PARAM_FILE"

# Deployment name
DEPLOYMENT_NAME="bookstore-$ENVIRONMENT-$(date +%Y%m%d%H%M%S)"
print_info "Deployment name: $DEPLOYMENT_NAME"

# Execute deployment
echo ""
if [[ "$WHAT_IF" == true ]]; then
    print_warning "Running in WHAT-IF mode (no changes will be made)"
    echo ""

    az deployment sub what-if \
        --name "$DEPLOYMENT_NAME" \
        --location "$DEPLOYMENT_LOCATION" \
        --template-file "$INFRA_DIR/main.bicep" \
        --parameters "$PARAM_FILE"
else
    print_info "Starting deployment..."
    echo ""

    # Confirmation for production
    if [[ "$ENVIRONMENT" == "prod" ]]; then
        print_warning "You are about to deploy to PRODUCTION!"
        read -p "Are you sure you want to continue? (yes/no): " confirm
        if [[ "$confirm" != "yes" ]]; then
            print_info "Deployment cancelled"
            exit 0
        fi
    fi

    az deployment sub create \
        --name "$DEPLOYMENT_NAME" \
        --location "$DEPLOYMENT_LOCATION" \
        --template-file "$INFRA_DIR/main.bicep" \
        --parameters "$PARAM_FILE" \
        --output table

    if [[ $? -eq 0 ]]; then
        echo ""
        print_success "Deployment completed successfully!"

        # Show outputs
        echo ""
        print_info "Deployment outputs:"
        az deployment sub show \
            --name "$DEPLOYMENT_NAME" \
            --query properties.outputs \
            --output table
    else
        print_error "Deployment failed"
        exit 1
    fi
fi

echo ""
print_success "Done!"
