cd java\build\intermediates\classes\release

CALL javap -s com.razerzone.store.sdk.engine.unity.DebugInput > ..\..\..\..\..\signature_debuginput.txt

CALL javap -s com.razerzone.store.sdk.engine.unity.InputView > ..\..\..\..\..\signature_inputview.txt

CALL javap -s com.razerzone.store.sdk.engine.unity.MainActivity > ..\..\..\..\..\signature_mainactivity.txt

CALL javap -s com.razerzone.store.sdk.engine.unity.Plugin > ..\..\..\..\..\signature_plugin.txt

CALL javap -s com.razerzone.store.sdk.engine.unity.StoreFacadeWrapper > ..\..\..\..\..\signature_storefacadewrapper.txt
