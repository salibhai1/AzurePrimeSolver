﻿DECLARE @CurrentMigration [nvarchar](max)

IF object_id('[dbo].[__MigrationHistory]') IS NOT NULL
    SELECT @CurrentMigration =
        (SELECT TOP (1) 
        [Project1].[MigrationId] AS [MigrationId]
        FROM ( SELECT 
        [Extent1].[MigrationId] AS [MigrationId]
        FROM [dbo].[__MigrationHistory] AS [Extent1]
        WHERE [Extent1].[ContextKey] = N'PrimeSolverRepository.Migrations.Configuration'
        )  AS [Project1]
        ORDER BY [Project1].[MigrationId] DESC)

IF @CurrentMigration IS NULL
    SET @CurrentMigration = '0'

IF @CurrentMigration < '201605041554380_InitialCreate'
BEGIN
    CREATE TABLE [dbo].[PrimeNumberCandidates] (
        [Number] [int] NOT NULL,
        [IsPrime] [bit],
        CONSTRAINT [PK_dbo.PrimeNumberCandidates] PRIMARY KEY ([Number])
    )
    CREATE TABLE [dbo].[__MigrationHistory] (
        [MigrationId] [nvarchar](150) NOT NULL,
        [ContextKey] [nvarchar](300) NOT NULL,
        [Model] [varbinary](max) NOT NULL,
        [ProductVersion] [nvarchar](32) NOT NULL,
        CONSTRAINT [PK_dbo.__MigrationHistory] PRIMARY KEY ([MigrationId], [ContextKey])
    )
    INSERT [dbo].[__MigrationHistory]([MigrationId], [ContextKey], [Model], [ProductVersion])
    VALUES (N'201605041554380_InitialCreate', N'PrimeSolverRepository.Migrations.Configuration',  0x1F8B0800000000000400CD57C96EDB3010BD17E83F083CA766964B1BC82912272982360BA2B4775A1ABB44B9A8241544DFD6433FA9BFD0A1569BB29CA5055AF86251336F66DE6CD4AF1F3FE3F70F5244F7602CD76A4AF626BB240295EA8CABE594146EF1E62D797FF4FA557C96C987E84B2B77E0E55053D929F9EA5C7E48A94DBF82647622796AB4D50B3749B5A42CD3747F77F71DDDDBA3801004B1A228BE2D94E312AA077C9C699542EE0A262E7506C236E7F826A950A32B26C1E62C8529B931A89868813EDF42AE2D77DA94243A169CA133098805899852DA3187AE1E7EB69038A3D532C9F18089BB3207945B3061A109E1B0177F6A34BBFB3E1ADA2BB65069619D96CF04DC3B68E8A1A1FA8B48261D7D48E01912ED4A1F754562C3DF5521E760664C653C630E89080D1FCE84F14A237C4F36A1EC441B6577BAB2C1EAF2BF9D6856085718982A289C6102158BB9E0E94728EFF43750535508B11A05C681EFD60EF0E8C6E81C8C2B6F61D1C4563B4422BAAE4B43E54E35D0AB23BE50EE609F4457E8049B0BE88A65859D0403830FA0C060DCD90D730E0CE6FA4A2B18580F6C5DD88AA5D6D889D602985AD78A699FB66132B15B1CE3687B4B46AD178207B721B3D8114D72B1AE9C292060BA369280DB064FA2DEC3BAEB3696C4E6B0BA00FA3EA775A3B703818E4C84F892E5398EA69509D19C44493D1E666F92E7378DAC31686A37F44EE76D670993CF9610BC45D3E8E93937D69D32C7E6CC97CC2C9303B1ADE91A49456B7A5B46C2F6E813D46AFBFF2B084F6AEA10B667FC1C4990A05CC50774AE8E4C98014C35DC996066A417675A14526DEBEB6D285D97ADC27487439C980651856CD2019DC18C0973B5ADEE4391CE7A57FF419DC74DCD3DBE1E0745588B109CCDFA9E67BE0093D23A90132F3049BE8B99E0186F2F70C9145F8075F52826B89EF683F5FAFFAC3A6A6D265EB0EFFED96EE19EE9C16679D6C698F335886A82FFC1F6184EADE7AF83276F83BA18A7249B6B0CA70E6064BFBC70710C1B25A6ABB7CDF8142C5FF610FEEEA920F515D883B632176AA1DB642003AB1EB52241AE2EC1310C801D1BC7172C75F83A056BB19549F485890245CE30D2EC425D172E2FDCB1B520E7A25C8D37A6DBED57DB71DDE7F83AF74FF66F84806E5639B85627051759E7F7F9B056C7207C4D35B723F40AEFDF08B72C3BA4E135690CA8A1EF147250194EA93B90B9F0F571AD12760F2FF10DAF3F9F60C9D2B29D77E3208F27629DF6F894B3A561D23618BDBEFF82A2FE13EAE83726AC49C2740D0000 , N'6.1.3-40302')
END

