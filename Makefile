.PHONY: tests run sbom clean

# Default target
all: run

# Run the test suite
tests:
	dotnet test knucklebones/KnuckleBonesTests/KnuckleBonesTests.csproj

# Run the application
run:
	dotnet run --project knucklebones/KnuckleBonesApp/KnuckleBonesApp.csproj

# Generate SBOM and scan for vulnerabilities
# Requires syft and grype to be installed
sbom:
	@echo "Generating SBOM with syft..."
	syft dir:. -o cyclonedx-json > sbom.json
	@echo "Scanning SBOM for vulnerabilities with grype..."
	grype sbom.json

# Clean build artifacts
clean:
	find . -type d -name "bin" -exec rm -rf {} +
	find . -type d -name "obj" -exec rm -rf {} +
	rm -f sbom.json
