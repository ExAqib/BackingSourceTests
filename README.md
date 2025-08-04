This repository contains unit test cases for NCache backing source. 
It contains test cases for both ReadThu and WriteThru.
I have made it public so that my I can share it with my team members. 
Directory structure:
└── exaqib-backingsourcetests/
    ├── BackingSourceTests.csproj
    ├── BackingSourceTests.sln
    ├── TestBase.cs
    ├── ReadThru/
    │   ├── ReadThruBase.cs
    │   ├── Atomic/
    │   │   ├── ReadThruAtomic.cs
    │   │   └── ReadThruAtomicMetadata.cs
    │   └── Bulk/
    │       ├── ReadThruBulk.cs
    │       └── ReadThruBulkMetadata.cs
    └── WriteThru/
        ├── WriteThruBase.cs
        ├── Atomic/
        │   ├── WriteThruAtomic.cs
        │   └── WriteThruMetaTestCases.cs
        └── Bulk/
            ├── WriteThruBulk.cs
            ├── WriteThruBulkBase.cs
            └── WriteThruBulkMeta.cs
