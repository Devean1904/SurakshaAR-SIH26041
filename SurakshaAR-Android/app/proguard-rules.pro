# SurakshaAR ProGuard Rules

# Retrofit
-keepattributes Signature
-keepattributes *Annotation*
-keep class com.surakshaar.data.model.** { *; }
-keep class com.surakshaar.data.api.GenericMessageResponse { *; }
-keep class com.surakshaar.data.api.CertificateVerifyResponse { *; }
-dontwarn retrofit2.**
-keep class retrofit2.** { *; }

# Gson
-keep class com.google.gson.** { *; }
-keepclassmembers class * {
    @com.google.gson.annotations.SerializedName <fields>;
}

# Ed25519
-keep class net.i2p.crypto.eddsa.** { *; }

# ZXing
-keep class com.google.zxing.** { *; }

# OkHttp
-dontwarn okhttp3.**
-dontwarn okio.**

# ARCore
-keep class com.google.ar.core.** { *; }
