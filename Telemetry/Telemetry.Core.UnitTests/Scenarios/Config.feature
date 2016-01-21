Feature: Config
	
@config
Scenario Template: able to retrieve app setting value
	Given key "<Key>"
	When I get value
	Then the result should be "<Value>"

Examples: 
| Key             | Value  |
| EnvironmentName | dev    |
| missing         | <null> |
