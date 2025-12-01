---
apply: always
---

* Write unit tests in XUnit, except instead of using Substitute.For<Interface> use GetSubstitute<Interface>  and Substitute.For<ILogger<Interface>> use GetTypedLogger<Interface>
* Use the NSubtitute library for mocking
* Always specify the full types for variables never use var
* Test classes should be derived from TestBase

