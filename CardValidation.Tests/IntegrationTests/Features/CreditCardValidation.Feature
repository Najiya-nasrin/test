@allure.owner:Faheem
Feature: Credit Card Validation API
  As a user of the credit card validation system
  I want to verify that the API correctly validates various credit card inputs
  So that only valid cards are accepted

  Scenario Outline: Validate Credit Card
    Given this is test case "<TestCaseName>"
    And I prepare a credit card with:
      | Owner       | Number            | Cvv  | IssueDate |
      | <Owner>     | <Number>          | <Cvv> | <IssueDate> |
    When I send the card to the validation API
    Then the response status should be <ExpectedStatus>
    And the response body should contain "<ExpectedResult>"

   Examples:
    # Valid card tests
    | TestCaseName                  | Owner        | Number             | Cvv  | IssueDate | ExpectedStatus | ExpectedResult |
    | Valid Visa Card               | John Doe     | 4111111111111111   | 123  | 12/30     | 200            | 10             |
    | Valid MasterCard              | Alice Smith  | 5555555555554444   | 321  | 11/29     | 200            | 20             |
    | Valid American Express        | Bob Johnson  | 378282246310005    | 1234 | 06/28     | 200            | 30             |

    # Missing field tests
    | Missing Owner                 |              | 4111111111111111   | 123  | 12/30     | 400            | Owner          |
    | Missing Number                | Jane Doe     |                    | 123  | 12/30     | 400            | Number         |
    | Missing CVV                   | Jane Doe     | 4111111111111111   |      | 12/30     | 400            | Cvv            |
    | Missing IssueDate             | Jane Doe     | 4111111111111111   | 123  |           | 400            | date           |

    # Invalid input formats
    | Invalid Card Number           | Test User    | 1234567890123456   | 123  | 12/30     | 400            | Number         |
    | Invalid CVV length Visa       | Clara Wells  | 4111111111111111   | 12   | 12/30     | 400            | Cvv            |
    | Expired Card Past Date        | Expired User | 4111111111111111   | 123  | 01/21     | 400            | date           |

    # Owner field issues
    | Owner Contains Number         | Jane 4111    | 4111111111111111   | 123  | 12/30     | 400            | Owner          |
    | Owner Contains Card Number    | John 4111111 | 4111111111111111   | 123  | 12/30     | 400            | Owner          |

    # Edge case formats
    | CVV with special chars        | Jane Doe     | 4111111111111111   | 12@# | 12/30     | 400            | Cvv            |
    | Card Number with spaces       | John Doe     | 4111 1111 1111 1111 | 123  | 12/30     | 400            | Number        |

    # Multiple validation errors
    | Invalid CVV and Date Format   | John Doe     | 4111111111111111   | 12   | 99-99     | 400            | {"Cvv":["Wrong cvv"],"Date":["Wrong date"]}        |
    | Invalid Owner and CVV         | John 123     | 4111111111111111   | 12   | 12/30     | 400            | {"Cvv":["Wrong cvv"],"Owner":["Wrong owner"]}      |
    | Invalid All Fields            |              | 123456             | abc  | invalid   | 400            | {"Cvv":["Wrong cvv"],"Date":["Wrong date"],"Owner":["Owner is required"],"Number":["Wrong number"]} |


