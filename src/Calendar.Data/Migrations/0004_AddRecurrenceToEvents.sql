-- All three are null for a single (non-recurring) event.
-- RecurrenceFrequency: 1 = Daily, 2 = Weekly.
Alter Table Events Add Column RecurrenceFrequency Integer Null Check (RecurrenceFrequency In (1, 2));
Alter Table Events Add Column RecurrenceInterval Integer Null;
Alter Table Events Add Column RecurrenceCount Integer Null;
