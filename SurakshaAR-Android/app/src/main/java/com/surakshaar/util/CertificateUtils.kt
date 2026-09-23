package com.surakshaar.util

import net.i2p.crypto.eddsa.EdDSASecurityProvider
import net.i2p.crypto.eddsa.KeyPairGenerator
import net.i2p.crypto.eddsa.spec.EdDSANamedCurveTable
import net.i2p.crypto.eddsa.spec.EdDSAPrivateKeySpec
import java.security.KeyPair
import java.security.MessageDigest
import java.security.Signature
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

object CertificateUtils {

    init {
        java.security.Security.addProvider(EdDSASecurityProvider())
    }

    fun generateCertificateId(): String {
        val datePart = SimpleDateFormat("yyyyMMdd", Locale.US).format(Date())
        val hexPart = buildString {
            repeat(8) {
                append("0123456789ABCDEF".random())
            }
        }
        return "CERT-$datePart-$hexPart"
    }

    fun generateCertificateId(employeeId: String, moduleId: String): String {
        val datePart = SimpleDateFormat("yyyyMMdd", Locale.US).format(Date())
        val hexPart = buildString {
            repeat(8) {
                append("0123456789ABCDEF".random())
            }
        }
        return "$datePart-$hexPart"
    }

    fun generateQrPayload(certificateId: String): String {
        return "SURAKSHA:CERT:$certificateId"
    }

    fun generateQrPayload(certificateId: String, employeeId: String, moduleId: String, score: Int): String {
        return "SURAKSHA:CERT:$certificateId:EMP:$employeeId:MOD:$moduleId:SCORE:$score"
    }

    fun generateSignature(data: String): String {
        return sha256Hash(data)
    }

    fun generateKeyPair(): KeyPair {
        val kpg = KeyPairGenerator()
        kpg.initialize(EdDSANamedCurveTable.getByName("Ed25519"), java.security.SecureRandom())
        return kpg.generateKeyPair()
    }

    fun generateEd25519Signature(data: String, privateKey: ByteArray): String {
        val keySpec = EdDSAPrivateKeySpec(
            privateKey,
            EdDSANamedCurveTable.getByName("Ed25519")
        )
        val privateKeyObj = net.i2p.crypto.eddsa.EdDSAPrivateKey(keySpec)
        val signature = Signature.getInstance("EdDSA")
        signature.initSign(privateKeyObj)
        signature.update(data.toByteArray(Charsets.UTF_8))
        val signedBytes = signature.sign()
        return signedBytes.joinToString("") { "%02x".format(it) }
    }

    fun sha256Hash(data: String): String {
        val digest = MessageDigest.getInstance("SHA-256")
        val hashBytes = digest.digest(data.toByteArray(Charsets.UTF_8))
        return hashBytes.joinToString("") { "%02x".format(it) }
    }
}
