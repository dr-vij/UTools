//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.5.0
//     from Assets/Plugins/UTools/InputSystem/InputActions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace UTools.Input
{
    public partial class @InputActions: IInputActionCollection2, IDisposable
    {
        public InputActionAsset asset { get; }
        public @InputActions()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""InputActions"",
    ""maps"": [
        {
            ""name"": ""GestureActions"",
            ""id"": ""56661286-639e-479d-a086-84a642abb239"",
            ""actions"": [
                {
                    ""name"": ""Pointer"",
                    ""type"": ""PassThrough"",
                    ""id"": ""9eb45181-1a19-40d1-a479-c4322cfc51f9"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""Mouse"",
                    ""id"": ""c5e88b36-41c0-41ef-b7db-774a84a057a5"",
                    ""path"": ""PointerInput"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Contact"",
                    ""id"": ""f40cb589-d9d9-4d02-b70e-2019907d9b47"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Position"",
                    ""id"": ""79ea67f1-e56a-4252-879d-eca1a360e36a"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Pen"",
                    ""id"": ""807fa0b6-b0bb-4cd9-877c-d325bf7c9a25"",
                    ""path"": ""PointerInput"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Contact"",
                    ""id"": ""5200187f-de07-4f9f-ba50-b98856d10333"",
                    ""path"": ""<Pen>/tip"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Position"",
                    ""id"": ""e4748ec6-1570-4ca1-964c-00415edf3636"",
                    ""path"": ""<Pen>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Tilt"",
                    ""id"": ""3c366c60-cfbd-45ed-a85e-417a2f5b9676"",
                    ""path"": ""<Pen>/tilt"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Radius"",
                    ""id"": ""128622a0-acfd-4342-a6b9-73401bebd613"",
                    ""path"": ""<Pen>/radius"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Pressure"",
                    ""id"": ""268f357a-c79d-4e79-8db1-172878ab318e"",
                    ""path"": ""<Pen>/pressure"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Twist"",
                    ""id"": ""d7d2bd5b-d755-433d-8786-af83dbe3f320"",
                    ""path"": ""<Pen>/twist"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Touch 0"",
                    ""id"": ""4f4962a4-6833-40c1-83f0-5febea9aab6d"",
                    ""path"": ""PointerInput"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Contact"",
                    ""id"": ""4ac40104-02b1-4512-8f87-62b36f30dea3"",
                    ""path"": ""<Touchscreen>/touch0/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Position"",
                    ""id"": ""4186096f-60ca-47ec-8fe1-b77f20f448ff"",
                    ""path"": ""<Touchscreen>/touch0/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Radius"",
                    ""id"": ""83522ae8-b71b-4159-abd3-f71271234ba8"",
                    ""path"": ""<Touchscreen>/touch0/radius"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Pressure"",
                    ""id"": ""d71734cd-0984-49b8-af8c-fba6a7ba272c"",
                    ""path"": ""<Touchscreen>/touch0/pressure"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""InputId"",
                    ""id"": ""10edb234-b451-4a71-8594-773fcb4de944"",
                    ""path"": ""<Touchscreen>/touch0/touchId"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Touch 1"",
                    ""id"": ""c5d26dc8-adf9-4b6a-96bb-265591f5588d"",
                    ""path"": ""PointerInput"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Contact"",
                    ""id"": ""6b2e86f6-4e45-4847-ad8c-02ec2345638a"",
                    ""path"": ""<Touchscreen>/touch1/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Position"",
                    ""id"": ""709a1bf4-6535-4ad6-9740-ed7aa3d38f91"",
                    ""path"": ""<Touchscreen>/touch1/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Radius"",
                    ""id"": ""f5a7be40-7543-42ae-b00e-5fcc4c2943a6"",
                    ""path"": ""<Touchscreen>/touch1/radius"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Pressure"",
                    ""id"": ""2232164f-be86-49b4-a842-9f4749879bcc"",
                    ""path"": ""<Touchscreen>/touch1/pressure"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""InputId"",
                    ""id"": ""acf190a5-b32d-47e1-93d1-70dc2ba71dbf"",
                    ""path"": ""<Touchscreen>/touch1/touchId"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Touch 2"",
                    ""id"": ""267bbd8c-cc49-4d43-aea6-eced0d5f07ce"",
                    ""path"": ""PointerInput"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Contact"",
                    ""id"": ""e37f05af-82f9-4e2a-9b24-8564df689c92"",
                    ""path"": ""<Touchscreen>/touch2/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Position"",
                    ""id"": ""3ed55caf-c81d-4572-9e85-0ff617d8f525"",
                    ""path"": ""<Touchscreen>/touch2/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Radius"",
                    ""id"": ""684bc411-ed12-4939-8a5a-17104592c5ec"",
                    ""path"": ""<Touchscreen>/touch2/radius"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Pressure"",
                    ""id"": ""2c47220f-cbe7-48f6-ac64-bf90228314b1"",
                    ""path"": ""<Touchscreen>/touch2/pressure"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""InputId"",
                    ""id"": ""66950a37-4291-4aaf-a34a-48887f6a6dbb"",
                    ""path"": ""<Touchscreen>/touch2/touchId"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Touch 3"",
                    ""id"": ""f3bd94bd-7b68-4fc7-890c-0a3d77efa5e0"",
                    ""path"": ""PointerInput"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Contact"",
                    ""id"": ""69dbba0c-eb28-41ee-aaf4-4c3e8d8b4da5"",
                    ""path"": ""<Touchscreen>/touch3/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Position"",
                    ""id"": ""2b461be6-c34d-40e9-87c4-4911d043fd43"",
                    ""path"": ""<Touchscreen>/touch3/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Radius"",
                    ""id"": ""4b237643-9de5-43fb-8066-ea78a7b749b6"",
                    ""path"": ""<Touchscreen>/touch3/radius"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Pressure"",
                    ""id"": ""b2697dae-64b0-4edd-897f-557f52852f5b"",
                    ""path"": ""<Touchscreen>/touch3/pressure"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""InputId"",
                    ""id"": ""70dd9e57-385a-462f-a246-e9683e77bc4e"",
                    ""path"": ""<Touchscreen>/touch3/touchId"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Touch 4"",
                    ""id"": ""8b4585db-63e6-4ba6-8790-2ff7520e39fc"",
                    ""path"": ""PointerInput"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Contact"",
                    ""id"": ""f802d953-220c-490c-b582-51436b63d949"",
                    ""path"": ""<Touchscreen>/touch4/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Position"",
                    ""id"": ""7971a320-9294-4468-97da-22cd05043354"",
                    ""path"": ""<Touchscreen>/touch4/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Radius"",
                    ""id"": ""6670a843-7a06-4452-a57c-6362789b5dde"",
                    ""path"": ""<Touchscreen>/touch4/radius"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Pressure"",
                    ""id"": ""06cc4045-0732-4c74-8511-32fd9f2b02c9"",
                    ""path"": ""<Touchscreen>/touch4/pressure"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""InputId"",
                    ""id"": ""c90a5b02-f2fa-4476-81d3-cca1a13f91d9"",
                    ""path"": ""<Touchscreen>/touch4/touchId"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Touch 5"",
                    ""id"": ""36fe4fc9-08c1-4cf3-aa26-43aeb2abdddb"",
                    ""path"": ""PointerInput"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Contact"",
                    ""id"": ""c7800c60-bb9e-4908-a772-3a1017f81c8a"",
                    ""path"": ""<Touchscreen>/touch5/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Position"",
                    ""id"": ""db59cce9-0732-45b3-ad28-117f3314663a"",
                    ""path"": ""<Touchscreen>/touch5/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Radius"",
                    ""id"": ""36831d23-e94a-444e-a0c2-3bf26c382def"",
                    ""path"": ""<Touchscreen>/touch5/radius"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Pressure"",
                    ""id"": ""f10213b8-007d-4404-a1e3-31cf22b5eea1"",
                    ""path"": ""<Touchscreen>/touch5/pressure"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""InputId"",
                    ""id"": ""2f0664f9-264c-495a-9ec9-6fde6c24af81"",
                    ""path"": ""<Touchscreen>/touch5/touchId"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Touch 6"",
                    ""id"": ""fb48d32e-091c-448b-9057-b36b7c1b5ec7"",
                    ""path"": ""PointerInput"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Contact"",
                    ""id"": ""9adb5e8d-24e1-42f9-ab77-016e9f1a12be"",
                    ""path"": ""<Touchscreen>/touch6/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Position"",
                    ""id"": ""e1c76976-2d9a-4b55-9836-85362daa167a"",
                    ""path"": ""<Touchscreen>/touch6/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Radius"",
                    ""id"": ""80295b1b-d745-40eb-a630-78abf88b3aad"",
                    ""path"": ""<Touchscreen>/touch6/radius"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Pressure"",
                    ""id"": ""983203aa-ae20-40b0-b6c9-32febf71f3cc"",
                    ""path"": ""<Touchscreen>/touch6/pressure"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""InputId"",
                    ""id"": ""38d84564-4386-4c28-a773-d4f1886cecea"",
                    ""path"": ""<Touchscreen>/touch6/touchId"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Touch 7"",
                    ""id"": ""d49ee540-1034-4a6b-ba4d-886241bb763f"",
                    ""path"": ""PointerInput"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Contact"",
                    ""id"": ""1e8eada7-4463-4944-b0d4-70e276150ad5"",
                    ""path"": ""<Touchscreen>/touch7/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Position"",
                    ""id"": ""4d5e9cb4-9142-439e-860a-f6bd0d04387c"",
                    ""path"": ""<Touchscreen>/touch7/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Radius"",
                    ""id"": ""71983677-cfa5-4f8d-8a01-704022c95447"",
                    ""path"": ""<Touchscreen>/touch7/radius"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Pressure"",
                    ""id"": ""97366249-5650-4dee-a4e5-80d243a8f873"",
                    ""path"": ""<Touchscreen>/touch7/pressure"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""InputId"",
                    ""id"": ""d38cfefc-5813-498a-8684-2d8dfa149fbb"",
                    ""path"": ""<Touchscreen>/touch7/touchId"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Touch 8"",
                    ""id"": ""35e871f4-cfc0-404a-984c-5eeafca936fc"",
                    ""path"": ""PointerInput"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Contact"",
                    ""id"": ""9d673aab-b89f-429d-be54-f25fb130d7e2"",
                    ""path"": ""<Touchscreen>/touch8/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Position"",
                    ""id"": ""c69828cc-e05a-4ec5-a319-00f27861cb66"",
                    ""path"": ""<Touchscreen>/touch8/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Radius"",
                    ""id"": ""c8ae4962-051f-41c8-8e63-d95fc93bd259"",
                    ""path"": ""<Touchscreen>/touch8/radius"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Pressure"",
                    ""id"": ""22bb631c-bd59-4e63-9655-1509bbd70f2a"",
                    ""path"": ""<Touchscreen>/touch8/pressure"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""InputId"",
                    ""id"": ""0db7fc55-f800-4588-ae52-a925c07beb1b"",
                    ""path"": ""<Touchscreen>/touch8/touchId"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Touch 9"",
                    ""id"": ""a9f173f1-3974-40a1-ae92-67e77671e380"",
                    ""path"": ""PointerInput"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Contact"",
                    ""id"": ""257bd320-9c68-4a05-8588-70e0e085ffcc"",
                    ""path"": ""<Touchscreen>/touch9/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Position"",
                    ""id"": ""132c7f6b-20d0-4933-a22c-115a3e412504"",
                    ""path"": ""<Touchscreen>/touch9/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Radius"",
                    ""id"": ""d67331c3-82a8-4ff5-a34c-ff6209acc344"",
                    ""path"": ""<Touchscreen>/touch9/radius"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Pressure"",
                    ""id"": ""e2bdfffd-9b14-4e03-8b24-c5256259a791"",
                    ""path"": ""<Touchscreen>/touch9/pressure"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""InputId"",
                    ""id"": ""a09f0def-fa0f-4cc8-9829-854406ff9c87"",
                    ""path"": ""<Touchscreen>/touch9/touchId"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
            // GestureActions
            m_GestureActions = asset.FindActionMap("GestureActions", throwIfNotFound: true);
            m_GestureActions_Pointer = m_GestureActions.FindAction("Pointer", throwIfNotFound: true);
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(asset);
        }

        public InputBinding? bindingMask
        {
            get => asset.bindingMask;
            set => asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices
        {
            get => asset.devices;
            set => asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

        public bool Contains(InputAction action)
        {
            return asset.Contains(action);
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            return asset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enable()
        {
            asset.Enable();
        }

        public void Disable()
        {
            asset.Disable();
        }

        public IEnumerable<InputBinding> bindings => asset.bindings;

        public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
        {
            return asset.FindAction(actionNameOrId, throwIfNotFound);
        }

        public int FindBinding(InputBinding bindingMask, out InputAction action)
        {
            return asset.FindBinding(bindingMask, out action);
        }

        // GestureActions
        private readonly InputActionMap m_GestureActions;
        private List<IGestureActionsActions> m_GestureActionsActionsCallbackInterfaces = new List<IGestureActionsActions>();
        private readonly InputAction m_GestureActions_Pointer;
        public struct GestureActionsActions
        {
            private @InputActions m_Wrapper;
            public GestureActionsActions(@InputActions wrapper) { m_Wrapper = wrapper; }
            public InputAction @Pointer => m_Wrapper.m_GestureActions_Pointer;
            public InputActionMap Get() { return m_Wrapper.m_GestureActions; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(GestureActionsActions set) { return set.Get(); }
            public void AddCallbacks(IGestureActionsActions instance)
            {
                if (instance == null || m_Wrapper.m_GestureActionsActionsCallbackInterfaces.Contains(instance)) return;
                m_Wrapper.m_GestureActionsActionsCallbackInterfaces.Add(instance);
                @Pointer.started += instance.OnPointer;
                @Pointer.performed += instance.OnPointer;
                @Pointer.canceled += instance.OnPointer;
            }

            private void UnregisterCallbacks(IGestureActionsActions instance)
            {
                @Pointer.started -= instance.OnPointer;
                @Pointer.performed -= instance.OnPointer;
                @Pointer.canceled -= instance.OnPointer;
            }

            public void RemoveCallbacks(IGestureActionsActions instance)
            {
                if (m_Wrapper.m_GestureActionsActionsCallbackInterfaces.Remove(instance))
                    UnregisterCallbacks(instance);
            }

            public void SetCallbacks(IGestureActionsActions instance)
            {
                foreach (var item in m_Wrapper.m_GestureActionsActionsCallbackInterfaces)
                    UnregisterCallbacks(item);
                m_Wrapper.m_GestureActionsActionsCallbackInterfaces.Clear();
                AddCallbacks(instance);
            }
        }
        public GestureActionsActions @GestureActions => new GestureActionsActions(this);
        public interface IGestureActionsActions
        {
            void OnPointer(InputAction.CallbackContext context);
        }
    }
}
