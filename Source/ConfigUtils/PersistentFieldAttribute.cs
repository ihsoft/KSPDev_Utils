// Kerbal Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using System;

namespace KSPDev.ConfigUtils {

/// <summary>A simple attribute for the fields that need (de)serialization.</summary>
/// <remarks>
/// <para>
/// The readonly fields cannot be restored from a persistent state. However, they can be written
/// out.
/// </para>
/// <para>
/// By default the ordinal values are handled via <see cref="StandardOrdinaryTypesProto"/>
/// and the collection fields via <see cref="GenericCollectionTypeProto"/>. These proto handlers can
/// be changed in the annotation by assigning properties
/// <see cref="BasePersistentFieldAttribute.ordinaryTypeProto"/> and/or
/// <see cref="BasePersistentFieldAttribute.collectionTypeProto"/>.
/// </para>
/// </remarks>
/// <example>
/// Below is a simple usage of the attribute.
/// <code><![CDATA[
/// class ClassWithPersistentFields {
///   [PersistentField("my/listField", isCollection = true)]
///   private List<string> sampleList = new List<string>();
/// 
///   internal struct ComplexType {
///     [PersistentField("val1", group = "nevermind")]
///     public bool boolVal;
///     [PersistentField("val2")]
///     public Color colorVal;
///   }
/// 
///   [PersistentField("my/setField", isCollection = true, group = "abc")]
///   private HashSet<ComplexType> sampleSet = new HashSet<ComplexType>();
/// 
///   void SaveConfigs() {
///     // Save a default group of fields: only field "sampleList" qualifies.
///     ConfigAccessor.WriteFieldsIntoFile("settings.cfg", instance: this);
///     /* The following structure in the file will be created:
///      * {
///      *   my
///      *   {
///      *     listField: string1
///      *     listField: string2
///      *   }
///      * }
///      */
/// 
///     // Save a specific group of fields: only field "sampleSet" belongs to group "abc".
///     sampleSet.Add(new ComplexType() { boolVal = true, colorVal = Color.black });
///     sampleSet.Add(new ComplexType() { boolVal = false, colorVal = Color.white });
///     ConfigAccessor.WriteFieldsIntoFile("settings.cfg", instance: this, group: "abc");
///     /* The following structure in the file will be created:
///      * {
///      *     setField
///      *     {
///      *       val1: True
///      *       val2: 0,0,0,1
///      *     }
///      *     setField
///      *     {
///      *       val1: false
///      *       val2: 1,1,1,1
///      *     }
///      *   }
///      * }
///      */
///   }
/// }
/// ]]></code>
/// <para>
/// Note that the group is ignored in the nested types. I.e. in <c>ComplexType</c> in this case.
/// However, if <c>ComplexType</c> was an immediate target of the <c>WriteFieldsIntoFile</c> call
/// then the group would be considered.
/// </para>
/// <para>
/// Visibility of the annotated field is also important. The persistent field attributes are only
/// visible in the child class if they were public or protected in the parent. The private field
/// annotations are not inherited and need to be handled at the level of the declaring class.
/// </para>
/// <code><![CDATA[
/// class Parent {
///   [PersistentField("parent_private")]
///   private int field1 = 1;
/// 
///   [PersistentField("parent_protected")]
///   protected int field2 = 2;
/// 
///   [PersistentField("parent_public")]
///   public int field3 = 3;
/// }
/// 
/// class Child : Parent {
///   [PersistentField("child_private")]
///   private int field1 = 10;
/// 
///   void SaveConfig() {
///     // Save all fields in the inherited type. 
///     ConfigAccessor.WriteFieldsIntoFile("settings.cfg", instance: this);
///     /* The following structure in the file will be created:
///      * {
///      *     parent_protected: 2
///      *     parent_public: 3
///      *     child_private: 10
///      * }
///      */
/// 
///     // Save all fields in the base type. 
///     ConfigAccessor.WriteFieldsIntoFile("settings.cfg", instance: (Parent) this);
///     /* The following structure in the file will be created:
///      * {
///      *     parent_private: 1
///      *     parent_protected: 2
///      *     parent_public: 3
///      * }
///      */
///   }
/// }
/// ]]></code>
/// <para>
/// The code above implies that in common case the unsealed class should put the private fields in a
/// group other than default to avoid settings collision.
/// </para>
/// <para>
/// Instead of creating nested classes with attributed fields, you can makea class that implements
/// KSP interface <see cref="IConfigNode"/>. In this case the Save/load methid will be invoked when
/// (de)serializing the field.
/// </para>
/// <code><![CDATA[
/// public class NodeCustomType : IConfigNode {
///   public virtual void Save(ConfigNode node) {
///   }
///   public virtual void Load(ConfigNode node) {
///   }
///
///   [PersistentField("custom_class")]
///   public NodeCustomType field1;
/// }
/// ]]></code>
/// <para>
/// In case of your type is really simple, and you can serialize it into a plain string, you may
/// choose to implement <see cref="IPersistentField"/> instead. It works in a similar way but the
/// source/target of the persistense is a string instead of a config node.
/// </para>
/// </example>
/// <seealso cref="ConfigAccessor"/>
/// <seealso cref="AbstractOrdinaryValueTypeProto"/>
/// <seealso cref="AbstractCollectionTypeProto"/>
/// <seealso cref="IPersistentField"/>
/// <seealso cref="StdPersistentGroups"/>
/// <seealso href="https://kerbalspaceprogram.com/api/interface_i_config_node.html">KSP: IConfigNode</seealso>
[AttributeUsage(AttributeTargets.Field)]
public sealed class PersistentFieldAttribute : BasePersistentFieldAttribute {
  /// <summary>Specifies if the annotated field is a collection of values.</summary>
  /// <value><c>true</c> if the field is a collection.</value>
  public bool isCollection {
    set { collectionTypeProto = value ? typeof(GenericCollectionTypeProto) : null; }
    get { return collectionTypeProto != null; }
  }

  /// <summary>Creates attribute for a persistent field of standard KSP types.</summary>
  /// <inheritdoc/>
  /// <seealso cref="StandardOrdinaryTypesProto"/>
  /// <seealso cref="GenericCollectionTypeProto"/>
  public PersistentFieldAttribute(string cfgPath) : base(cfgPath) {
    ordinaryTypeProto = typeof(StandardOrdinaryTypesProto);
    isCollection = false;
  }
}

}  // namespace
