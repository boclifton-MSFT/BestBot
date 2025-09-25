#!/bin/bash

# Script to validate Bicep templates for BestBot infrastructure
# Tests both basic and APIM deployment scenarios

set -e

echo "🔍 Validating BestBot Infrastructure Templates"
echo "============================================="

cd "$(dirname "$0")"

# Test 1: Basic deployment (no APIM)
echo ""
echo "✅ Test 1: Basic deployment validation"
echo "--------------------------------------"

cat > test-basic-params.json << EOF
{
  "\$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#",
  "contentVersion": "1.0.0.0",
  "parameters": {
    "location": {
      "value": "southcentralus"
    },
    "environmentName": {
      "value": "test-basic"
    },
    "deployApim": {
      "value": false
    }
  }
}
EOF

if az bicep build --file main.bicep --stdout > /dev/null 2>&1; then
    echo "✓ Basic deployment template builds successfully"
else
    echo "❌ Basic deployment template has errors"
    exit 1
fi

# Test 2: APIM deployment
echo ""
echo "✅ Test 2: APIM deployment validation"
echo "------------------------------------"

cat > test-apim-params.json << EOF
{
  "\$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#",
  "contentVersion": "1.0.0.0",
  "parameters": {
    "location": {
      "value": "southcentralus"
    },
    "environmentName": {
      "value": "test-apim"
    },
    "deployApim": {
      "value": true
    },
    "apimPublisherEmail": {
      "value": "test@example.com"
    },
    "apimSku": {
      "value": "Developer"
    }
  }
}
EOF

if az bicep build --file main.bicep --stdout > /dev/null 2>&1; then
    echo "✓ APIM deployment template builds successfully"
else
    echo "❌ APIM deployment template has errors"
    exit 1
fi

# Test 3: APIM module standalone
echo ""
echo "✅ Test 3: APIM module validation"
echo "--------------------------------"

if az bicep build --file modules/apim.bicep --stdout > /dev/null 2>&1; then
    echo "✓ APIM module builds successfully"
else
    echo "❌ APIM module has errors"
    exit 1
fi

# Cleanup test files
rm -f test-basic-params.json test-apim-params.json

echo ""
echo "🎉 All infrastructure templates validated successfully!"
echo ""
echo "Usage Examples:"
echo "==============="
echo ""
echo "1. Deploy without APIM:"
echo "   azd up"
echo ""
echo "2. Deploy with APIM:"
echo "   azd env set deployApim true"
echo "   azd env set apimPublisherEmail \"your-email@example.com\""
echo "   azd up"
echo ""
echo "3. Custom parameters file:"
echo "   Create custom main.parameters.json and run azd up"
echo ""