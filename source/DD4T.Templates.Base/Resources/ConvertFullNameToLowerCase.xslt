<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0"
 xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:variable
     name="lcase">abcdefghijklmnopqrstuvwxyz</xsl:variable>
  <xsl:variable
     name="ucase">ABCDEFGHIJKLMNOPQRSTUVWXYZ</xsl:variable>

  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="*">
    <xsl:element name="{translate(local-name(), $ucase, $lcase)}">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

</xsl:stylesheet>