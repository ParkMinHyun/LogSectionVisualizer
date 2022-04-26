namespace LogVisualizer.data {
    internal class SampleData {

        public const string json = 
            "{\n  " +
                "\"filters\": [\n    " +
                    "{\n      " +
                        "\"name\": \"openCamera\",\n      " +
                        "\"start\": \"[CAMFWKPI] OpenCamera E\",\n      " +
                        "\"end\": \"[CAMFWKPI] OpenCamera X\"\n    " +
                    "},\n    " +
                    "{\n      " +
                        "\"name\": \"createCaptureSession\",\n      " +
                        "\"start\": \"[CAMFWKPI] createCaptureSession E\",\n      " +
                        "\"end\": \"[CAMFWKPI] createCaptureSession X\"\n    " +
                    "},\n    " +
                   "{\n      " +
                        "\"name\": \"initializeMaker\",\n      " +
                        "\"start\": \"initializeProcessingPhotoMaker E\",\n      " +
                        "\"end\": \"initializeMaker X\"\n    },\n    " +
                    "{\n      " +
                        "\"name\": \"firstPreviewFrame\",\n      " +
                        "\"start\": \"[CAMFWKPI] startPreviewRepeating\",\n      " +
                        "\"end\": \"[CAMFWKPI] first producePreviewFrame E\"\n    " +
                    "},\n    {\n      " +
                        "\"name\": \"total\",\n      " +
                        "\"start\": \"[CAMFWKPI] openCamDevice E\",\n      " +
                        "\"end\": \"[CAMFWKPI] first producePreviewFrame X\"\n    " +
                    "}\n  " +
                "]\n" +
            "}";
    }
}
