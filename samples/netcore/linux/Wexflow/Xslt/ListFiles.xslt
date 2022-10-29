<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="/">
    <root>
      <WexflowProcessing>
        <xsl:for-each select="//WexflowProcessing/Workflow/Files//File">
          <xsl:choose>
            <xsl:when test="@name = 'file1.txt'">
              <File taskId="{@taskId}" name="{@name}" renameTo="file1_renamed.txt" todo="toRename" from="app1" />
            </xsl:when>
            <xsl:when test="@name = 'file2.txt'">
              <File taskId="{@taskId}" name="{@name}" renameTo="file2_renamed.txt" todo="toSend" from="app2" />
            </xsl:when>
            <xsl:when test="@name = 'file3.txt'">
              <File taskId="{@taskId}" name="{@name}" renameTo="file3_renamed.txt" todo="toDownload" from="app3" />
            </xsl:when>
            <xsl:when test="@name = 'file4.txt'">
              <File taskId="{@taskId}" name="{@name}" renameTo="file4_renamed.txt" todo="toDownload" from="app4" />
            </xsl:when>
          </xsl:choose>
        </xsl:for-each>
      </WexflowProcessing>
    </root>
  </xsl:template>
</xsl:stylesheet>
