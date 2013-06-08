using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using MetroExplorer.Common;
using Windows.UI.Xaml.Documents;

namespace MetroExplorer.Common
{
    /// <summary>
    /// Wrapper pour <see cref="RichTextBlock"/> qui crée autant de colonnes de dépassement
    /// supplémentaires que nécessaire pour recevoir le contenu disponible.
    /// </summary>
    /// <example>
    /// Ce qui suit crée une collection de colonnes d'une largeur de 400 pixels et espacées de 50 pixels
    /// pour contenir le contenu lié aux données arbitraires :
    /// <code>
    /// <RichTextColumns>
    ///     <RichTextColumns.ColumnTemplate>
    ///         <DataTemplate>
    ///             <RichTextBlockOverflow Width="400" Margin="50,0,0,0"/>
    ///         </DataTemplate>
    ///     </RichTextColumns.ColumnTemplate>
    ///     
    ///     <RichTextBlock Width="400">
    ///         <Paragraph>
    ///             <Run Text="{Binding Content}"/>
    ///         </Paragraph>
    ///     </RichTextBlock>
    /// </RichTextColumns>
    /// </code>
    /// </example>
    /// <remarks>Généralement utilisé dans une zone à défilement horizontal dans laquelle une quantité illimitée
    /// d'espace permet de créer toutes les colonnes requises. Lors de son utilisation dans un espace à défilement
    /// vertical, il n'y a jamais de colonnes supplémentaires.</remarks>
    [Windows.UI.Xaml.Markup.ContentProperty(Name = "RichTextContent")]
    public sealed class RichTextColumns : Panel
    {
        /// <summary>
        /// Identifie la propriété de dépendance <see cref="RichTextContent"/>.
        /// </summary>
        public static readonly DependencyProperty RichTextContentProperty =
            DependencyProperty.Register("RichTextContent", typeof(RichTextBlock),
            typeof(RichTextColumns), new PropertyMetadata(null, ResetOverflowLayout));

        /// <summary>
        /// Identifie la propriété de dépendance <see cref="ColumnTemplate"/>.
        /// </summary>
        public static readonly DependencyProperty ColumnTemplateProperty =
            DependencyProperty.Register("ColumnTemplate", typeof(DataTemplate),
            typeof(RichTextColumns), new PropertyMetadata(null, ResetOverflowLayout));

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="RichTextColumns"/>.
        /// </summary>
        public RichTextColumns()
        {
            this.HorizontalAlignment = HorizontalAlignment.Left;
        }

        /// <summary>
        /// Obtient ou définit le contenu de texte enrichi à utiliser en tant que première colonne.
        /// </summary>
        public RichTextBlock RichTextContent
        {
            get { return (RichTextBlock)GetValue(RichTextContentProperty); }
            set { SetValue(RichTextContentProperty, value); }
        }

        /// <summary>
        /// Obtient ou définit le modèle utilisé pour créer des
        /// instances <see cref="RichTextBlockOverflow"/> supplémentaires.
        /// </summary>
        public DataTemplate ColumnTemplate
        {
            get { return (DataTemplate)GetValue(ColumnTemplateProperty); }
            set { SetValue(ColumnTemplateProperty, value); }
        }

        /// <summary>
        /// Invoqué lorsque le modèle de contenu ou de dépassement est modifié pour recréer la disposition des colonnes.
        /// </summary>
        /// <param name="d">Instance de <see cref="RichTextColumns"/> où la modification a été
        /// effectuée.</param>
        /// <param name="e">Données d'événement décrivant le changement spécifique.</param>
        private static void ResetOverflowLayout(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // En cas de modifications importantes, reconstruit entièrement la disposition des colonnes
            var target = d as RichTextColumns;
            if (target != null)
            {
                target._overflowColumns = null;
                target.Children.Clear();
                target.InvalidateMeasure();
            }
        }

        /// <summary>
        /// Répertorie les colonnes de dépassement déjà créées. Une relation 1:1 doit être maintenue avec
        /// les instances de la collection <see cref="Panel.Children"/> suivant l'enfant
        /// RichTextBlock initial.
        /// </summary>
        private List<RichTextBlockOverflow> _overflowColumns = null;

        /// <summary>
        /// Détermine si des colonnes de dépassement supplémentaires sont nécessaires et si des colonnes existantes peuvent
        /// être supprimées.
        /// </summary>
        /// <param name="availableSize">Taille de l'espace disponible, utilisé pour limiter le
        /// nombre de colonnes supplémentaires pouvant être créées.</param>
        /// <returns>Taille résultante du contenu d'origine auquel s'ajoutent éventuellement les colonnes supplémentaires.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (this.RichTextContent == null) return new Size(0, 0);

            // Vérifie que le RichTextBlock est un enfant, en déduisant de l'absence de
            // liste de colonnes supplémentaires que cela n'a pas encore
            // été fait
            if (this._overflowColumns == null)
            {
                Children.Add(this.RichTextContent);
                this._overflowColumns = new List<RichTextBlockOverflow>();
            }

            // Commence par mesurer le contenu RichTextBlock d'origine
            this.RichTextContent.Measure(availableSize);
            var maxWidth = this.RichTextContent.DesiredSize.Width;
            var maxHeight = this.RichTextContent.DesiredSize.Height;
            var hasOverflow = this.RichTextContent.HasOverflowContent;

            // Vérifie qu'il y a suffisamment de colonnes de dépassement
            int overflowIndex = 0;
            while (hasOverflow && maxWidth < availableSize.Width && this.ColumnTemplate != null)
            {
                // Utilise les colonnes de dépassement existantes jusqu'à épuisement, puis en crée
                // d'autres à l'aide du modèle fourni
                RichTextBlockOverflow overflow;
                if (this._overflowColumns.Count > overflowIndex)
                {
                    overflow = this._overflowColumns[overflowIndex];
                }
                else
                {
                    overflow = (RichTextBlockOverflow)this.ColumnTemplate.LoadContent();
                    this._overflowColumns.Add(overflow);
                    this.Children.Add(overflow);
                    if (overflowIndex == 0)
                    {
                        this.RichTextContent.OverflowContentTarget = overflow;
                    }
                    else
                    {
                        this._overflowColumns[overflowIndex - 1].OverflowContentTarget = overflow;
                    }
                }

                // Mesure la nouvelle colonne et envisage la répétition, si nécessaire
                overflow.Measure(new Size(availableSize.Width - maxWidth, availableSize.Height));
                maxWidth += overflow.DesiredSize.Width;
                maxHeight = Math.Max(maxHeight, overflow.DesiredSize.Height);
                hasOverflow = overflow.HasOverflowContent;
                overflowIndex++;
            }

            // Déconnecte les colonnes supplémentaires de la chaîne de dépassement, les supprime de la liste privée
            // de colonnes et les supprime en tant qu'enfants
            if (this._overflowColumns.Count > overflowIndex)
            {
                if (overflowIndex == 0)
                {
                    this.RichTextContent.OverflowContentTarget = null;
                }
                else
                {
                    this._overflowColumns[overflowIndex - 1].OverflowContentTarget = null;
                }
                while (this._overflowColumns.Count > overflowIndex)
                {
                    this._overflowColumns.RemoveAt(overflowIndex);
                    this.Children.RemoveAt(overflowIndex + 1);
                }
            }

            // Indique la taille déterminée finale
            return new Size(maxWidth, maxHeight);
        }

        /// <summary>
        /// Réorganise le contenu d'origine et toutes les colonnes supplémentaires.
        /// </summary>
        /// <param name="finalSize">Définit la taille de la zone où les enfants doivent être
        /// réorganisés.</param>
        /// <returns>Taille de la zone réellement requise par les enfants.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            double maxWidth = 0;
            double maxHeight = 0;
            foreach (var child in Children)
            {
                child.Arrange(new Rect(maxWidth, 0, child.DesiredSize.Width, finalSize.Height));
                maxWidth += child.DesiredSize.Width;
                maxHeight = Math.Max(maxHeight, child.DesiredSize.Height);
            }
            return new Size(maxWidth, maxHeight);
        }
    }
}
