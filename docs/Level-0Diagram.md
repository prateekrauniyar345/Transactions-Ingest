# Level-0 Diagram

This diagram shows the system as one process, its user, and the three transaction sources. Source systems are currently mocked inside the backend; the diagram shows their logical role.

![Level-0 system diagram](<level-0 diagram.png>)

The system stores transaction state, audit history, and import execution records in SQLite. Those internal details appear in the [data flow diagram](DataFlowDiagram.md). Angular integration will be added later.
